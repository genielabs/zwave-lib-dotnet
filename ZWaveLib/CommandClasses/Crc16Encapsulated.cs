/*
  This file is part of ZWaveLib (https://github.com/genielabs/zwave-lib-dotnet)

  Copyright (2012-2018) G-Labs (https://github.com/genielabs)

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
*/

/*
 *     Author: Alexander Sidorenko http://bounz.net
 *     Project Homepage: https://github.com/genielabs/zwave-lib-dotnet
 */

using System;
using System.Linq;

namespace ZWaveLib.CommandClasses
{
    public class Crc16Encapsulated : ICommandClass
    {
        public CommandClass GetClassId()
        {
            return CommandClass.Crc16Encapsulated;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            NodeEvent zevent = null;
            byte cmdType = message[1];
            switch (cmdType)
            {
            case 0x01:
                zevent = GetCrc16EncapEvent(node, message);
                break;
            }
            return zevent;
        }

        #region Private Helpers

        private NodeEvent GetCrc16EncapEvent(ZWaveNode node, byte[] message)
        {
            // calculate CRC
            var messageToCheckLength = message.Length - 2;
            byte[] messageCrc = new byte[2];
            Array.Copy(message, messageToCheckLength, messageCrc, 0, 2);
            byte[] toCheck = new byte[messageToCheckLength];
            Array.Copy(message, 0, toCheck, 0, messageToCheckLength);
            short crcToCheck = CalculateCrcCcit(toCheck);
            byte[] x = Int16ToBytes(crcToCheck);

            if (!x.SequenceEqual(messageCrc))
            {
                Utility.logger.Warn(String.Format("Bad CRC in message {0}. CRC is {1} but should be {2}", BitConverter.ToString(message), BitConverter.ToString(x), BitConverter.ToString(messageCrc)));
                return null;
            }

            byte[] encapsulatedMessage = new byte[message.Length - 2 - 2];
            Array.Copy(message, 2, encapsulatedMessage, 0, message.Length - 2 - 2);

            return ProcessEncapsulatedMessage(node, encapsulatedMessage);
        }

        private NodeEvent ProcessEncapsulatedMessage(ZWaveNode node, byte[] encapMessage)
        {
            Utility.logger.Debug(String.Format("CRC16 encapsulated message: {0}", BitConverter.ToString(encapMessage)));
            NodeEvent nodeEvent = null;
            byte cmdClass = encapMessage[0];
            var cc = CommandClassFactory.GetCommandClass(cmdClass);
            if (cc == null)
            {
                Utility.logger.Error(String.Format("Can't find CommandClass handler for command class {0}", cmdClass));
            }
            else
            {
                nodeEvent = cc.GetEvent(node, encapMessage);
            }
            return nodeEvent;
        }

        private byte[] Int16ToBytes(Int16 value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                var t = bytes[0];
                bytes[0] = bytes[1];
                bytes[1] = t;
            }
            return bytes;
        }

        private short CalculateCrcCcit(byte[] args)
        {
            int crc = 0x1D0F;
            int polynomial = 0x1021;
            foreach (byte b in args)
            {
                for (int i = 0; i < 8; i++)
                {
                    bool bit = ((b >> (7 - i) & 1) == 1);
                    bool c15 = ((crc >> 15 & 1) == 1);
                    crc <<= 1;
                    // If coefficient of bit and remainder polynomial = 1 xor crc with polynomial
                    if (c15 ^ bit)
                    {
                        crc ^= polynomial;
                    }
                }
            }
            crc &= 0xffff;
            return (short)crc;
        }

        #endregion

    }
}