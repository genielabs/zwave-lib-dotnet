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
    public enum IrrigationValveErrorStatusMask
    {
        None = 0,

        /// <summary>
        /// Short circuit has been detected.
        /// </summary>
        ShortCircuit = 1,

        /// <summary>
        /// Current high threshold has been detected.
        /// </summary>
        CurrentTooHigh = 2,

        /// <summary>
        /// Current low threshold has been detected.
        /// </summary>
        CurrentTooLow = 4,

        /// <summary>
        /// Maximum flow has been detected.
        /// </summary>
        MaximumFlow = 8,

        /// <summary>
        /// Flow high threshold has been detected.
        /// </summary>
        FlowTooHigh = 16,

        /// <summary>
        /// Flow low threshold has been detected.
        /// </summary>
        FlowTooLow = 32
    }
}
