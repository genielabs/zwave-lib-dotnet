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

using System.Collections.Generic;

using ZWaveLib.Values;

namespace ZWaveLib.CommandClasses
{
    public class ThermostatSetPoint : ICommandClass
    {
        public enum Value
        {
            Unused = 0x00,
            Heating = 0x01,
            Cooling = 0x02,
            Unused03 = 0x03,
            Unused04 = 0x04,
            Unused05 = 0x05,
            Unused06 = 0x06,
            Furnace = 0x07,
            DryAir = 0x08,
            MoistAir = 0x09,
            AutoChangeover = 0x0A,
            HeatingEconomy = 0x0B,
            CoolingEconomy = 0x0C,
            HeatingAway = 0x0D
        }

        public CommandClass GetClassId()
        {
            return CommandClass.ThermostatSetPoint;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            ZWaveValue zvalue = ZWaveValue.ExtractValueFromBytes(message, 4);
            var setPoint = GetSetPointData(node);
            setPoint.Precision = zvalue.Precision;
            setPoint.Scale = zvalue.Scale;
            setPoint.Size = zvalue.Size;
            setPoint.Value = zvalue.Value;
            // convert from Fahrenheit to Celsius if needed
            var ptype = new {
                Type = (Value)message[2],
                Value = (zvalue.Scale == (int)ZWaveTemperatureScaleType.Fahrenheit ? SensorValue.FahrenheitToCelsius(zvalue.Value) : zvalue.Value)
            };
            return new NodeEvent(node, EventParameter.ThermostatSetPoint, ptype, 0);
        }

        public static ZWaveMessage Get(ZWaveNode node, Value ptype)
        {
            return node.SendDataRequest(new byte[] {
                (byte)CommandClass.ThermostatSetPoint,
                (byte)Command.ThermostatSetPointGet,
                (byte)ptype
            });
        }

        public static ZWaveMessage Set(ZWaveNode node, Value ptype, double temperature)
        {
            List<byte> message = new List<byte>();
            message.AddRange(new byte[] {
                (byte)CommandClass.ThermostatSetPoint,
                (byte)Command.ThermostatSetPointSet,
                (byte)ptype
            });
            var setPoint = ThermostatSetPoint.GetSetPointData(node);
            message.AddRange(ZWaveValue.GetValueBytes(temperature, setPoint.Precision, setPoint.Scale, setPoint.Size));
            return node.SendDataRequest(message.ToArray());
        }

        public static ZWaveValue GetSetPointData(ZWaveNode node)
        {
            return (ZWaveValue)node.GetData("SetPoint", new ZWaveValue()).Value;
        }

    }
}
