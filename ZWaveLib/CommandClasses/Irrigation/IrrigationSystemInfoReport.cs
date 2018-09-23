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

namespace ZWaveLib.CommandClasses.Irrigation
{
    public class IrrigationSystemInfoReport
    {
        /// <summary>
        /// Indicate if a master valve is supported by the device.
        /// </summary>
        public bool IsMasterValueSupported { get; set; }

        /// <summary>
        /// Total number of zone valves supported by the device.
        /// </summary>
        public int TotalNumberOfValves { get; set; }

        /// <summary>
        /// Total number of valve tables that can be created/stored in the device.
        /// </summary>
        public int TotalNumberOfValveTables { get; set; }

        /// <summary>
        /// Maximum number of entries per valve table supported by the device.
        /// Must be in range 1..15.
        /// </summary>
        public int ValveTableMaxSize { get; set; }
    }
}
