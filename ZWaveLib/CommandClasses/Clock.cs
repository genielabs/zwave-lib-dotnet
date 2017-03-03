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
 *             Ben Voss
 *     Project Homepage: https://github.com/genielabs/zwave-lib-dotnet
 */

using System.Collections.Generic;

namespace ZWaveLib.CommandClasses
{
    public class Clock : ICommandClass
    {
        public CommandClass GetClassId()
        {
            return CommandClass.Clock;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            NodeEvent nodeEvent = null;
            byte cmdType = message[1];
            if ((message.Length > 0) && (cmdType == (byte)Command.ClockReport))
            {
                var clockValue = ClockValue.Parse (message);
                nodeEvent = new NodeEvent(node, EventParameter.Clock, clockValue, 0);
            }
            return nodeEvent;
        }

        public static ZWaveMessage Set(ZWaveNode node, ClockValue value)
        {
            List<byte> message = new List<byte> ();
            message.AddRange (new byte [] {
                (byte)CommandClass.Clock,
                (byte)Command.ClockSet
            });
            message.AddRange (value.GetValueBytes ());

            return node.SendDataRequest(message.ToArray());
        }

        public static ZWaveMessage Get(ZWaveNode node)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.Clock, 
                (byte)Command.ClockGet 
            });
        }

    }
}

