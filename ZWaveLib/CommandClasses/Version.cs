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
*     Author: https://github.com/mdave
*     Project Homepage: https://github.com/genielabs/zwave-lib-dotnet
*/
using System;
using ZWaveLib.Values;

namespace ZWaveLib.CommandClasses
{
    public class Version : ICommandClass
    {
        public CommandClass GetClassId()
        {
            return CommandClass.Version;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            NodeEvent nodeEvent = null;
            Command type = (Command)message[1];

            if (type == Command.VersionCommandClassReport)
            {
                if (!Enum.IsDefined(typeof(CommandClass), message[2]))
                {
                    return nodeEvent;
                }
                CommandClass cmdClass = (CommandClass)message[2];
                VersionValue value = new VersionValue(cmdClass, message[3]);
                // Update node CC data
                var nodeCc = node.GetCommandClass(cmdClass);
                if (nodeCc != null)
                    nodeCc.Version = value.Version;
                // Set the VersionCommandClass event
                nodeEvent = new NodeEvent(node, EventParameter.VersionCommandClass, value, 0);
            }

            return nodeEvent;
        }

        public static void Get(ZWaveNode node, CommandClass cmdClass)
        {
            node.SendDataRequest(new byte[] { 
                (byte)CommandClass.Version, 
                (byte)Command.VersionCommandClassGet,
                (byte)cmdClass
            });
        }
    }
}
