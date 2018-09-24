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
 *     Author: https://github.com/snagytx
 *     Project Homepage: https://github.com/genielabs/zwave-lib-dotnet
 */

using ZWaveLib.Values;

namespace ZWaveLib.CommandClasses
{
    public class DoorLock : ICommandClass
    {
        public enum Value
        {
            Unsecured = 0x00,
            UnsecuredTimeout = 0x01,
            InsideUnsecured = 0x10,
            InsideUnsecuredTimeout = 0x11,
            OutsideUnsecured = 0x20,
            OutsideUnsecuredTimeout = 0x21,
            Secured = 0xFF}

        ;

        public enum Alarm
        {
            Locked = 0x01,
            Unlocked = 0x02,
            LockedFromOutside = 0x05,
            UnlockedByUser = 0x06,
            // with id message[16] <--- TODO: find a way to route this info
            UnatuthorizedUnlock = 0x0F}

        ;

        public CommandClass GetClassId()
        {
            return CommandClass.DoorLock;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            NodeEvent nodeEvent = null;
            byte cmdType = message[1];
            if (cmdType == (byte)Command.DoorLockReport)
            {
                nodeEvent = new NodeEvent(node, EventParameter.DoorLockStatus, message[2], 0);
            }
            return nodeEvent;
        }

        public static ZWaveMessage Get(ZWaveNode node)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.DoorLock, 
                (byte)Command.DoorLockGet
            });
        }

        public static ZWaveMessage Set(ZWaveNode node, Value value)
        {
            byte lockValue;
            if (value == DoorLock.Value.Secured)
                lockValue = 255;
            else
                lockValue = 0;

            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.DoorLock, 
                (byte)Command.DoorLockSet,
                (byte)lockValue
            });
        }
    }
}
