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

        public static void Get(ZWaveNode node)
        {
            node.SendDataRequest(new byte[] { 
                (byte)CommandClass.DoorLock, 
                (byte)Command.DoorLockGet
            });
        }

        public static void Set(ZWaveNode node, Value value)
        {
            byte lockValue;
            if (value == DoorLock.Value.Secured)
                lockValue = 255;
            else
                lockValue = 0;

            node.SendDataRequest(new byte[] { 
                (byte)CommandClass.DoorLock, 
                (byte)Command.DoorLockSet,
                (byte)lockValue
            });
        }
    }
}
