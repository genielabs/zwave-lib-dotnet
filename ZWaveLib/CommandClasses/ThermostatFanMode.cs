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

namespace ZWaveLib.CommandClasses
{

    public class ThermostatFanMode : ICommandClass
    {
        public enum Value
        {
            AutoLow = 0x00,
            OnLow = 0x01,
            AutoHigh = 0x02,
            OnHigh = 0x03,
            Unknown4 = 0x04,
            Unknown5 = 0x05,
            Circulate = 0x06
        }

        public CommandClass GetClassId()
        {
            return CommandClass.ThermostatFanMode;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            return new NodeEvent(node, EventParameter.ThermostatFanMode, (Value)message[2], 0);
        }

        public static ZWaveMessage Get(ZWaveNode node)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.ThermostatFanMode, 
                (byte)Command.BasicGet
            });
        }

        public static ZWaveMessage Set(ZWaveNode node, Value mode)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.ThermostatFanMode, 
                (byte)Command.BasicSet, 
                (byte)mode
            });
        }
    }

}
