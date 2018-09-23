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

using System.Collections.Generic;
using ZWaveLib.Values;

namespace ZWaveLib.CommandClasses.Irrigation
{
    public class IrrigationSystemConfig
    {
        /// <summary>
        /// This field is used to configure a delay in seconds between turning on the master valve and turning on any �zone� valve.
        /// </summary>
        public byte MasterValveDelay { get; set; }

        /// <summary>
        /// These field is used to configure the pressure high threshold at the receiving node in kPa.
        /// </summary>
        public double PressureHighThreshold { get; set; }

        /// <summary>
        /// These field is used to configure the pressure low threshold at the receiving node in kPa.
        /// </summary>
        public double PressureLowThreshold { get; set; }

        /// <summary>
        /// This field is used to configure optional sensors� polarity at the receiving node.
        /// </summary>
        public IrrigationSensorPolarityMask SensorPolarity { get; set; }

        
        public int MoistureSensorPolarity { get; set; }

        public byte[] ToByteArray()
        {
            var commandBytes = new List<byte> {MasterValveDelay};
            commandBytes.AddRange(ZWaveValue.GetValueBytes(PressureHighThreshold, 0x00));
            commandBytes.AddRange(ZWaveValue.GetValueBytes(PressureLowThreshold, 0x00));
            commandBytes.Add((byte) SensorPolarity);
            return commandBytes.ToArray();
        }

        internal static IrrigationSystemConfig Parse(byte[] message)
        {
            var systemconfig = new IrrigationSystemConfig
            {
                MasterValveDelay = message[2]
            };
            var highPressureThresholdValueOffset = 3;
            var highPressureThresholdValue = ZWaveValue.ExtractValueFromBytes(message, highPressureThresholdValueOffset + 1);
            systemconfig.PressureHighThreshold = highPressureThresholdValue.Value;

            var lowPressureThresholdValueOffset = highPressureThresholdValueOffset + 1 + highPressureThresholdValue.Size;
            var lowPressureThresholdValue = ZWaveValue.ExtractValueFromBytes(message, lowPressureThresholdValueOffset + 1);
            systemconfig.PressureLowThreshold = lowPressureThresholdValue.Value;

            var sensorPolarityOffset = lowPressureThresholdValueOffset + 1 + lowPressureThresholdValue.Size;
            systemconfig.SensorPolarity = (IrrigationSensorPolarityMask) message[sensorPolarityOffset];
            
            return systemconfig;
        }
    }
}
