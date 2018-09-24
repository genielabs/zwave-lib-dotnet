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
    public enum IrrigationSystemErrorMask
    {
        None = 0,

        /// <summary>
        /// The device has not been programmed
        /// </summary>
        NotProgrammed = 1,

        /// <summary>
        /// The device has experienced an emergency shutdown
        /// </summary>
        EmergencyShutdows = 2,

        /// <summary>
        /// The device�s pressure high threshold has been triggered
        /// </summary>
        PressureTooHigh = 4,

        /// <summary>
        /// The device�s pressure low threshold has been triggered
        /// </summary>
        PressureTooLow = 8,

        /// <summary>
        /// A valve or the master valve is reporting error
        /// </summary>
        ValveError = 16
    }
}
