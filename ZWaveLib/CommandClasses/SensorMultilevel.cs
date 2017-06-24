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
using ZWaveLib.Values;

namespace ZWaveLib.CommandClasses
{
    public class SensorMultilevel : ICommandClass
    {
        public CommandClass GetClassId()
        {
            return CommandClass.SensorMultilevel;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            NodeEvent nodeEvent = null;
            byte cmdType = message[1];
            if (cmdType == (byte)Command.SensorMultilevelReport)
            {
                var sensor = SensorValue.Parse(message);
                if (sensor.Parameter == ZWaveSensorParameter.Unknown)
                {
                    byte key = message[2];
                    nodeEvent = new NodeEvent(node, EventParameter.SensorGeneric, sensor.Value, 0);
                    Utility.logger.Error("Unhandled sensor parameter type: " + key);
                }
                else
                {
                    nodeEvent = new NodeEvent(node, sensor.EventType, sensor.Value, 0);
                }
            }
            return nodeEvent;
        }

        public static ZWaveMessage Get(ZWaveNode node)
        {
            return node.SendDataRequest(new[] { 
                (byte)CommandClass.SensorMultilevel, 
                (byte)Command.SensorMultilevelGet 
            });
        }

        public static ZWaveMessage Get(ZWaveNode node, ZWaveSensorParameter sensorType, byte scale = 0x00)
        {
            return node.SendDataRequest(new[] {
                (byte)CommandClass.SensorMultilevel,
                (byte)Command.SensorMultilevelGet,
                (byte)sensorType,
                (byte)(scale << 3)
            });
        }
    }
}
