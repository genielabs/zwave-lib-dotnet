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

using System.Collections.Generic;
using System.Dynamic;

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
            dynamic ptype = new ExpandoObject();
            ptype.Type = (Value)message[2];
            // convert from Fahrenheit to Celsius if needed
            ptype.Value = (zvalue.Scale == (int)ZWaveTemperatureScaleType.Fahrenheit
                ? SensorValue.FahrenheitToCelsius(zvalue.Value)
                : zvalue.Value);
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
