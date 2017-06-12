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

namespace ZWaveLib.Values
{

    public enum ZWaveSensorParameter
    {
        Unknown = -1,
        Temperature = 1,
        GeneralPurposeValue = 2,
        Luminance = 3,
        Power = 4,
        RelativeHumidity = 5,
        Velocity = 6,
        Direction = 7,
        AtmosphericPressure = 8,
        BarometricPressure = 9,
        SolarRadiation = 10,
        DewPoint = 11,
        RainRate = 12,
        TideLevel = 13,
        Weight = 14,
        Voltage = 15,
        Current = 16,
        Co2Level = 17,
        AirFlow = 18,
        TankCapacity = 19,
        Distance = 20,
        AnglePosition = 21,

        WaterFlow = 56, // 0x38
        WaterPressure = 57 // 0x39
    }

    public enum ZWaveTemperatureScaleType : int
    {
        Celsius,
        Fahrenheit
    }

    public class SensorValue
    {
        public EventParameter EventType = EventParameter.SensorGeneric;
        public ZWaveSensorParameter Parameter = ZWaveSensorParameter.Unknown;
        public double Value = 0d;

        public static SensorValue Parse(byte[] message)
        {
            SensorValue sensorValue = new SensorValue();
            //
            ZWaveValue zvalue = ZWaveValue.ExtractValueFromBytes(message, 4);
            //
            byte key = message[2];
            if (key == (byte)ZWaveSensorParameter.Temperature)
            {
                sensorValue.Parameter = ZWaveSensorParameter.Temperature;
                // convert from Fahrenheit to Celsius if needed
                sensorValue.Value = (zvalue.Scale == (int)ZWaveTemperatureScaleType.Fahrenheit ? SensorValue.FahrenheitToCelsius(zvalue.Value) : zvalue.Value);
                sensorValue.EventType = EventParameter.SensorTemperature;
            }
            else if (key == (byte)ZWaveSensorParameter.GeneralPurposeValue)
            {
                sensorValue.Parameter = ZWaveSensorParameter.GeneralPurposeValue;
                sensorValue.Value = zvalue.Value;
                sensorValue.EventType = EventParameter.SensorGeneric;
            }
            else if (key == (byte)ZWaveSensorParameter.Luminance)
            {
                sensorValue.Parameter = ZWaveSensorParameter.Luminance;
                sensorValue.Value = zvalue.Value;
                sensorValue.EventType = EventParameter.SensorLuminance;
            }
            else if (key == (byte)ZWaveSensorParameter.RelativeHumidity)
            {
                sensorValue.Parameter = ZWaveSensorParameter.RelativeHumidity;
                sensorValue.Value = zvalue.Value;
                sensorValue.EventType = EventParameter.SensorHumidity;
            }
            else if (key == (byte)ZWaveSensorParameter.Power)
            {
                //sensor.Value = BitConverter.ToUInt16(new byte[2] { message[12], message[11] }, 0) / 10D;
                //sensor.Value = ((double)int.Parse(
                //    message[12].ToString("X2") + message[13].ToString("X2") + message[14].ToString("X2"),
                //    System.Globalization.NumberStyles.HexNumber
                //    )) / 1000D;
                // TODO: this might be very buggy.... to be tested
                EnergyValue energy = EnergyValue.Parse(message);
                sensorValue.Parameter = ZWaveSensorParameter.Power;
                sensorValue.Value = energy.Value;
                sensorValue.EventType = EventParameter.MeterPower;
            }
            else if (key == (byte)ZWaveSensorParameter.WaterFlow) //todo
            {
                sensorValue.Parameter = ZWaveSensorParameter.WaterFlow;
                sensorValue.Value = zvalue.Value;
                sensorValue.EventType = EventParameter.WaterFlow;
            }
            else if (key == (byte)ZWaveSensorParameter.WaterPressure) //todo
            {
                sensorValue.Parameter = ZWaveSensorParameter.WaterPressure;
                sensorValue.Value = zvalue.Value;
                sensorValue.EventType = EventParameter.WaterPressure;
            }
            else
            {
                sensorValue.Value = zvalue.Value;
            }
            //
            return sensorValue;
        }

        public static double FahrenheitToCelsius(double temperature)
        {
            return ((5.0 / 9.0) * (temperature - 32.0));
        }
    }

}

