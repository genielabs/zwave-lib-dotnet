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
 *             Ben Voss
 *     Project Homepage: https://github.com/genielabs/zwave-lib-dotnet
 */

namespace ZWaveLib
{
    public class ScheduleStateValue
    {
        /// <summary>
        /// The setback in 1/10 degrees (Kelvin)
        /// </summary>
        /// <remarks>
        ///     Example:
        ///         0 = 0 degrees setback
        ///         1 = 0.1 degrees is added to the setpoint
        ///         2 = 0.2 degrees is added to the setpoint
        ///         -1 = 0.1 degrees is subtracted from the setpoint
        ///         -2 = 0.2 degrees is subtracted from the setpoint
        /// 
        ///     Null When the schedule state indicates FrostProtection, EntergySavingMode or is Unused.
        /// </remarks>
        /// <value>The setback.</value>
        public int? Setback { get; set; }

        public bool FrostProtectionMode { get; set; }

        public bool EnergySavingMode { get; set; }

        public bool Unused { get; set; }

        public static ScheduleStateValue Parse (byte b)
        {
            var value = new ScheduleStateValue ();

            var sb = (sbyte)b;
            if (sb >= -128 && sb <= 120) {
                value.Setback = sb;
            } else if (b == 0x79) {
                value.FrostProtectionMode = true;
            } else if (b == 0x7a) {
                value.EnergySavingMode = true;
            } else if (b == 0x7f) {
                value.Unused = true;
            }

            return value;
        }

        public byte GetValueByte ()
        {
            if (FrostProtectionMode) {
                return 0x79;
            } else if (EnergySavingMode) {
                return 0x7a;
            } else if (Unused) {
                return 0x7f;
            } else if (Setback >= -128 && Setback <= 120) {
                return (byte)(sbyte)Setback;
            }

            // reserved
            return 0x7e;
        }

        public override string ToString ()
        {
            return string.Format ("[ScheduleStateValue: Setback={0}, FrostProtectionMode={1}, EnergySavingMode={2}, Unused={3}]", Setback, FrostProtectionMode, EnergySavingMode, Unused);
        }

    }
}
