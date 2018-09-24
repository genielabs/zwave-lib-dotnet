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

using System;

namespace ZWaveLib.CommandClasses.Irrigation
{
    [Flags]
    public enum IrrigationSensorPolarityMask
    {
        None = 0,

        /// <summary>
        /// Rain Sensor Polarity (0 LOW, 1 HIGH)
        /// </summary>
        RainSensorPolarity = 1,

        /// <summary>
        /// Moisture Sensor Polarity (0 LOW, 1 HIGH)
        /// </summary>
        MoistureSensorPolarity = 2,

        /// <summary>
        /// This bit MUST be set to 1 to indicate that the other bits in the bitmask contain valid data.
        /// </summary>
        Valid = 128
    }
}
