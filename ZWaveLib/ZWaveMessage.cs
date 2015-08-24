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
using System.Threading;

namespace ZWaveLib
{

    /// <summary>
    /// Z-Wave message.
    /// </summary>
    public class ZWaveMessage
    {

        #region Private fields

        private const byte callbackStartId = 0x02;
        private static byte callbackIdSeq = ZWaveMessage.callbackStartId;
        internal ulong seqNumber = 0;
        internal ManualResetEvent sentAck = new ManualResetEvent(true);

        #endregion

        #region public static fields

        /// <summary>
        /// Ack message.
        /// </summary>
        public static byte[] Ack = new byte[] { (byte)FrameHeader.ACK };
        /// <summary>
        /// Nack message.
        /// </summary>
        public static byte[] Nack = new byte[] { (byte)FrameHeader.NAK };

        #endregion

        #region Public Fields

        /// <summary>
        /// Max resend attempts.
        /// </summary>
        public const int ResendAttemptsMax = 2;
        /// <summary>
        /// The send message timeout in milliseconds.
        /// </summary>
        public const int SendMessageTimeoutMs = 10000;
        /// <summary>
        /// The Z-Wave message frame header.
        /// </summary>
        public FrameHeader Header;

        /// <summary>
        /// Gets or sets the identifier of the node subject of this message.
        /// </summary>
        /// <value>The node identifier.</value>
        public byte NodeId { get; internal set; }

        /// <summary>
        /// Gets or sets the callback identifier.
        /// </summary>
        /// <value>The callback identifier.</value>
        public byte CallbackId { get; internal set; }

        /// <summary>
        /// The raw message bytes data.
        /// </summary>
        public readonly byte[] RawData;

        /// <summary>
        /// The sequence number of this message.
        /// </summary>
        public readonly ulong Seq;

        /// <summary>
        /// The timestamp.
        /// </summary>
        public readonly DateTime Timestamp = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the resend counter.
        /// </summary>
        /// <value>The resend counter.</value>
        public int ResendCount { get; internal set; }

        /// <summary>
        /// The message direction (Inboud/Outbound).
        /// </summary>
        public readonly MessageDirection Direction = MessageDirection.Outbound;
        /// <summary>
        /// The type of message (Request/Response).
        /// </summary>
        public readonly MessageType Type = MessageType.NotSet;
        /// <summary>
        /// The function.
        /// </summary>
        public readonly ZWaveFunction Function = ZWaveFunction.NotSet;
        /// <summary>
        /// The command class.
        /// </summary>
        public readonly CommandClass CommandClass = CommandClass.NotSet;
        /// <summary>
        /// The callback status.
        /// </summary>
        public readonly CallbackStatus CallbackStatus = CallbackStatus.NotSet;

        #endregion

        #region Public members

        /// <summary>
        /// Initializes a new instance of the <see cref="ZWaveLib.ZWaveMessage"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="direction">Direction.</param>
        /// <param name="generateCallback">If set to <c>true</c> generate callback.</param>
        public ZWaveMessage(byte[] message, MessageDirection direction = MessageDirection.Outbound, bool generateCallback = false)
        {
            Direction = direction;
            Header = (FrameHeader)message[0];
            RawData = message;

            if (direction == MessageDirection.Outbound)
            {
                if (generateCallback)
                {
                    CallbackId = GenerateCallbackId();
                    RawData[RawData.Length - 2] = CallbackId;
                }
                // Insert checksum
                RawData[RawData.Length - 1] = GenerateChecksum(RawData);
            }

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
                             && message.Length >= 6)
                    {
                        if (Direction == MessageDirection.Outbound)
                        {
                            NodeId = message[4];
                            CallbackId = (Function == ZWaveFunction.RequestNodeNeighborsUpdate) ? message[5] : message[6];
                        }
                        else
                        {
                            CallbackId = message[4];
                        }
                    }
                    else if ((Function == ZWaveFunction.NodeAdd || Function == ZWaveFunction.NodeRemove)
                             && message.Length == 9)
                    {
                        CallbackId = message[4];
                    }
                    else if (Function == ZWaveFunction.RequestNodeInfo || Function == ZWaveFunction.GetNodeProtocolInfo || Function == ZWaveFunction.GetRoutingInfo)
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

            if (seqNumber == long.MaxValue)
                seqNumber = 0;
            Seq = ++seqNumber;
        }

        /// <summary>
        /// Wait until this message transaction is completed.
        /// </summary>
        public ZWaveMessage Wait()
        {
            sentAck.WaitOne(SendMessageTimeoutMs);
            return this;
        }

        #endregion

        #region Public static utility functions

        /// <summary>
        /// Builds a Z-Wave SendData request message.
        /// </summary>
        /// <returns>The send data request.</returns>
        /// <param name="nodeId">Node identifier.</param>
        /// <param name="request">Request.</param>
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

        /// <summary>
        /// Verifies the checksum.
        /// </summary>
        /// <returns><c>true</c>, if checksum was verifyed, <c>false</c> otherwise.</returns>
        /// <param name="data">Data.</param>
        public static bool VerifyChecksum(byte[] data)
        {
            uint checksum = 0xff;
            for (int i = 1; i < (data.Length - 1); ++i)
            {
                checksum ^= data[i];
            }        
            return (checksum == data[data.Length - 1]);
        }

        /// <summary>
        /// Generates the callback identifier for an Outbound message.
        /// </summary>
        /// <returns>The callback identifier.</returns>
        private static byte GenerateCallbackId()
        {
            if (++callbackIdSeq > 0xFF)
            {
                callbackIdSeq = callbackStartId;
            }
            return callbackIdSeq;
        }

        /// <summary>
        /// Generates the checksum of an Outbound message.
        /// </summary>
        /// <returns>The checksum.</returns>
        /// <param name="data">Data.</param>
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

        #endregion

    }

}
