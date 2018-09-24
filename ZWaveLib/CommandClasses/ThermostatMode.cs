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

    public class ThermostatMode : ICommandClass
    {
        public enum Value
        {
            Off = 0x00,
            Heat = 0x01,
            Cool = 0x02,
            Auto = 0x03,
            AuxHeat = 0x04,
            Resume = 0x05,
            FanOnly = 0x06,
            Furnace = 0x07,
            DryAir = 0x08,
            MoistAir = 0x09,
            AutoChangeover = 0x0A,
            HeatEconomy = 0x0B,
            CoolEconomy = 0x0C,
            Away = 0x0D
        }

        public CommandClass GetClassId()
        {
            return CommandClass.ThermostatMode;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            return  new NodeEvent(node, EventParameter.ThermostatMode, (Value)message[2], 0);
        }

        public static ZWaveMessage Get(ZWaveNode node)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.ThermostatMode, 
                (byte)Command.BasicGet
            });
        }

        public static ZWaveMessage Set(ZWaveNode node, Value mode)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.ThermostatMode, 
                (byte)Command.BasicSet, 
                (byte)mode
            });
        }

        public static ZWaveMessage Set (ZWaveNode node, int channel, Value mode)
        {
            return node.SendDataRequest (new byte [] {
                (byte)CommandClass.MultiInstance,
                (byte)Command.MultiChannelEncapsulated,
                0,
                (byte)channel,
                (byte)CommandClass.ThermostatMode,
                (byte)Command.BasicSet,
                (byte)mode
            });

        }
    }
}
