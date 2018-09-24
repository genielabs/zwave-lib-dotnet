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
