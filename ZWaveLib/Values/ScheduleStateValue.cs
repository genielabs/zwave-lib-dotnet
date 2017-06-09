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
