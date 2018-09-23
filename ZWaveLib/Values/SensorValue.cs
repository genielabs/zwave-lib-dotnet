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

        WaterFlow = 56,     // 0x38
        WaterPressure = 57, // 0x39
        Ultraviolet = 27    // 0x1B
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
            var zvalue = ZWaveValue.ExtractValueFromBytes(message, 4);
            var sensorType = ZWaveSensorParameter.Unknown;
            if (Enum.IsDefined(typeof(ZWaveSensorParameter), (int)message[2]))
                sensorType = (ZWaveSensorParameter)message[2];

            var sensorValue = new SensorValue
            {
                Parameter = sensorType,
                Value = zvalue.Value
            };

            switch (sensorType)
            {
                case ZWaveSensorParameter.Temperature:
                    // convert from Fahrenheit to Celsius if needed
                    sensorValue.Value = zvalue.Scale == (int)ZWaveTemperatureScaleType.Fahrenheit
                        ? FahrenheitToCelsius(zvalue.Value)
                        : zvalue.Value;
                    sensorValue.EventType = EventParameter.SensorTemperature;
                    break;

                case ZWaveSensorParameter.GeneralPurposeValue:
                    sensorValue.EventType = EventParameter.SensorGeneric;
                    break;

                case ZWaveSensorParameter.Luminance:
                    sensorValue.EventType = EventParameter.SensorLuminance;
                    break;

                case ZWaveSensorParameter.RelativeHumidity:
                    sensorValue.EventType = EventParameter.SensorHumidity;
                    break;

                case ZWaveSensorParameter.Power:
                    //sensor.Value = BitConverter.ToUInt16(new byte[2] { message[12], message[11] }, 0) / 10D;
                    //sensor.Value = ((double)int.Parse(
                    //    message[12].ToString("X2") + message[13].ToString("X2") + message[14].ToString("X2"),
                    //    System.Globalization.NumberStyles.HexNumber
                    //    )) / 1000D;
                    // TODO: this might be very buggy.... to be tested
                    EnergyValue energy = EnergyValue.Parse(message);
                    sensorValue.Value = energy.Value;
                    sensorValue.EventType = EventParameter.MeterPower;
                    break;

                case ZWaveSensorParameter.WaterFlow:
                    sensorValue.EventType = EventParameter.WaterFlow;
                    break;

                case ZWaveSensorParameter.WaterPressure:
                    sensorValue.EventType = EventParameter.WaterPressure;
                    break;

                case ZWaveSensorParameter.Ultraviolet:
                    sensorValue.EventType = EventParameter.Ultraviolet;
                    break;

                // TODO: implement other Sensor Types
            }

            return sensorValue;
        }
        
        public static double FahrenheitToCelsius(double temperature)
        {
            return ((5.0 / 9.0) * (temperature - 32.0));
        }
    }
}
