/*
    This file is part of ZWaveLib Project source code.

    ZWaveLib is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ZWaveLib is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with ZWaveLib.  If not, see <http://www.gnu.org/licenses/>.  
*/

/*
 *     Author: Generoso Martello <gene@homegenie.it>
 *     Project Homepage: https://github.com/genielabs/zwave-lib-dotnet
 */

using System;

namespace ZWaveLib
{

    public enum QueryStage
    {
        NotSet,
        WaitAck,
        SendDataReady,
        WaitData,
        Complete,
        Error
    }

    public enum FrameHeader : byte
    {
        SOF = 0x01,
        ACK = 0x06,
        NAK = 0x15,
        CAN = 0x18
    }

    public enum MessageType : byte
    {
        Request = 0x00,
        Response = 0x01,
        NotSet = 0xFF
    }

    public class MessageReceivedEventArgs
    {
        public readonly ZWaveMessage Message;

        public MessageReceivedEventArgs(ZWaveMessage message)
        {
            Message = message;
        }
    }

    public class ZWaveMessage
    {
        private const byte CallbackStartId = 0x02;
        // 0x01 is reserved
        private static byte callbackIdSeq = ZWaveMessage.CallbackStartId;

        public static byte[] Ack = new byte[] { (byte)FrameHeader.ACK };
        public static byte[] Nack = new byte[] { (byte)FrameHeader.NAK };
        public const int ResendAttemptsMax = 3;

        public FrameHeader Header;

        public byte CallbackId { get; internal set; }

        public byte NodeId { get; internal set; }

        public readonly byte[] RawData;

        public readonly DateTime Timestamp = DateTime.UtcNow;
        public readonly int ResendCount = 0;

        public readonly MessageType Type = MessageType.NotSet;
        public readonly ZWaveFunction Function = ZWaveFunction.NotSet;
        public readonly CommandClass CommandClass = CommandClass.NotSet;
        public readonly CallbackStatus CallbackStatus = CallbackStatus.NotSet;

        public ZWaveMessage(byte[] message)
        {
            Header = (FrameHeader)message[0];
            RawData = message;
            if (Header == FrameHeader.SOF)
            {
                if (message.Length > 4)
                {
                    Enum.TryParse<MessageType>(message[2].ToString(), out Type);
                    Enum.TryParse<ZWaveFunction>(message[3].ToString(), out Function);
                }
                switch (Type)
                {
                case MessageType.Request:
                    if (Function == ZWaveFunction.SendData && message.Length == 8)
                    {
                        Enum.TryParse<CommandClass>(message[6].ToString(), out CommandClass);
                    }
                    else if (Function == ZWaveFunction.SendData && message.Length == 6)
                    {
                        Enum.TryParse<CallbackStatus>(message[4].ToString(), out CallbackStatus);
                    }
                    else if (Function == ZWaveFunction.SendData && message.Length == 7)
                    {
                        CallbackId = message[4];
                        Enum.TryParse<CallbackStatus>(message[5].ToString(), out CallbackStatus);
                    }
                    else if (Function == ZWaveFunction.SendData && message.Length > 6)
                    {
                        NodeId = message[4];
                        Enum.TryParse<CommandClass>(message[6].ToString(), out CommandClass);
                    }
                    else if (Function == ZWaveFunction.ApplicationCommandHandler && message.Length > 7)
                    {
                        NodeId = message[5];
                        Enum.TryParse<CommandClass>(message[7].ToString(), out CommandClass);
                    }
                    else if ((Function == ZWaveFunction.RequestNodeNeighborsUpdate || Function == ZWaveFunction.RequestNodeNeighborsUpdateOptions)
                             && message.Length == 7)
                    {
                        CallbackId = message[4];
                    }
                    else if ((Function == ZWaveFunction.NodeAdd || Function == ZWaveFunction.NodeRemove)
                             && message.Length == 9)
                    {
                        CallbackId = message[4];
                    }
                    else if (Function == ZWaveFunction.GetNodeProtocolInfo || Function == ZWaveFunction.GetRoutingInfo || Function == ZWaveFunction.RequestNodeNeighborsUpdate || Function == ZWaveFunction.RequestNodeNeighborsUpdateOptions)
                    {
                        NodeId = message[4];
                    }
                    break;
                case MessageType.Response:
                    if (message.Length == 6)
                    {
                        Enum.TryParse<CallbackStatus>(message[4].ToString(), out CallbackStatus);
                    }
                    break;
                }
            }
        }

        public static byte[] BuildSendDataRequest(byte nodeId, byte[] request)
        {
            byte[] header = new byte[] {
                (byte)FrameHeader.SOF, /* Start Of Frame */
                (byte)(request.Length + 7) /*packet len */,
                (byte)MessageType.Request, /* Type of message */
                (byte)ZWaveFunction.SendData /* func send data */,
                nodeId,
                (byte)(request.Length)
            };
            byte[] footer = new byte[] { 0x01 | 0x04, 0x00, 0x00 };
            byte[] message = new byte[header.Length + request.Length + footer.Length];// { 0x01 /* Start Of Frame */, 0x09 /*packet len */, 0x00 /* type req/res */, 0x13 /* func send data */, NodeId, 0x02, 0x31, 0x04, 0x01 | 0x04, 0x00, 0x00 };

            System.Array.Copy(header, 0, message, 0, header.Length);
            System.Array.Copy(request, 0, message, header.Length, request.Length);
            System.Array.Copy(footer, 0, message, message.Length - footer.Length, footer.Length);

            return message;
        }

        public void Prepare(bool generateCallback)
        {
            if (generateCallback)
            {
                CallbackId = GenerateCallbackId();
                RawData[RawData.Length - 2] = CallbackId;
            }
            // Insert checksum
            RawData[RawData.Length - 1] = GenerateChecksum(RawData);
        }

        public static bool VerifyChecksum(byte[] data)
        {
            uint checksum = 0xff;
            for (int i = 1; i < (data.Length - 1); ++i)
            {
                checksum ^= data[i];
            }        
            return (checksum == data[data.Length - 1]);
        }

        private static byte GenerateCallbackId()
        {
            if (++callbackIdSeq > 0xFF)
            {
                callbackIdSeq = CallbackStartId;
            }
            return callbackIdSeq;
        }

        private static byte GenerateChecksum(byte[] data)
        {
            int offset = 1;
            byte returnValue = data[offset];
            for (int i = offset + 1; i < data.Length - 1; i++)
            {
                // Xor bytes
                returnValue ^= data[i];
            }
            // Not result
            returnValue = (byte)(~returnValue);
            return returnValue;
        }

    }

}
