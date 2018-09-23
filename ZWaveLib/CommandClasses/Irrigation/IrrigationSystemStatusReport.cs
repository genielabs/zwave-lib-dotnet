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

using ZWaveLib.Values;

namespace ZWaveLib.CommandClasses.Irrigation
{
    public class IrrigationSystemStatusReport
    {
        /// <summary>
        /// The voltage level applied at the device.
        /// </summary>
        public int SystemVoltage { get; set; }

        /// <summary>
        /// This field is used to advertise if optional sensors are currently reporting values or detecting events at the device.
        /// </summary>
        public IrrigationSystemSensorStatusMask SensorStatus { get; set; }

        /// <summary>
        /// The Flow value measured by the flow sensor.
        /// </summary>
        public double Flow { get; set; }

        /// <summary>
        /// The Pressure value measured by the pressure sensor.
        /// </summary>
        public double Pressure { get; set; }

        /// <summary>
        /// Indicates how many hours are left in the �shut off� mode.
        /// </summary>
        public int ShutoffDuration { get; set; }

        /// <summary>
        /// This field is used to advertise if any system error is being active at the sending node.
        /// </summary>
        public IrrigationSystemErrorMask SystemErrorStatus { get; set; }

        /// <summary>
        /// This field is used to indicate if a master valve is currently open or closed.
        /// </summary>
        public bool IsMasterValveOpen { get; set; }

        /// <summary>
        /// This field is used to indicate the Valve ID of the first open zone valve currently On / Open.
        /// </summary>
        public int ValveId { get; set; }

        internal static IrrigationSystemStatusReport Parse(byte[] message)
        {
            var systemStatus = new IrrigationSystemStatusReport
            {
                SystemVoltage = message[2],
                SensorStatus = (IrrigationSystemSensorStatusMask)message[3]
            };
            var flowValueOffset = 4;
            var flowValue = ZWaveValue.ExtractValueFromBytes(message, flowValueOffset + 1);
            systemStatus.Flow = flowValue.Value;

            var pressureValueOffset = flowValueOffset + 1 + flowValue.Size;
            var pressureValue = ZWaveValue.ExtractValueFromBytes(message, pressureValueOffset + 1);
            systemStatus.Pressure = pressureValue.Value;

            var shutOffDurationOffset = pressureValueOffset + 1 + pressureValue.Size;
            systemStatus.ShutoffDuration = message[shutOffDurationOffset];
            systemStatus.SystemErrorStatus = (IrrigationSystemErrorMask)message[shutOffDurationOffset + 1];
            systemStatus.IsMasterValveOpen = message[shutOffDurationOffset + 2] == 1;
            systemStatus.ValveId = message[shutOffDurationOffset + 3];

            return systemStatus;
        }
    }
}
