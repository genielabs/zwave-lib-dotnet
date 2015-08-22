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

namespace ZWaveLib.CommandClasses
{
    public class SwitchMultilevel : ICommandClass
    {
        public CommandClass GetClassId()
        {
            return CommandClass.SwitchMultilevel;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            NodeEvent nodeEvent = null;
            byte cmdType = message[1];
            if (cmdType == (byte)Command.SwitchMultilevelReport || cmdType == (byte)Command.SwitchMultilevelSet) // some devices use this instead of report
            {
                int levelValue = (int)message[2];
                nodeEvent = new NodeEvent(node, EventParameter.Level, (double)levelValue, 0);
            }
            return nodeEvent;
        }

        public static void Set(ZWaveNode node, int value)
        {
            node.SendDataRequest(new byte[] { 
                (byte)CommandClass.SwitchMultilevel, 
                (byte)Command.SwitchMultilevelSet, 
                byte.Parse(value.ToString())
            });
        }

        public static void Get(ZWaveNode node)
        {
            node.SendDataRequest(new byte[] { 
                (byte)CommandClass.SwitchMultilevel, 
                (byte)Command.SwitchMultilevelGet 
            });
        }
    }
}

