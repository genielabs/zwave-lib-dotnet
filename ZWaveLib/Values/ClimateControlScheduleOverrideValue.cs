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
    public class ClimateControlScheduleOverrideValue
    {

        public OverrideType OverrideType { get; set; }

        public ScheduleStateValue ScheduleState { get; set; }

        public static ClimateControlScheduleOverrideValue Parse (byte [] bytes)
        {
            var result = new ClimateControlScheduleOverrideValue ();

            result.OverrideType = (OverrideType)bytes [2];
            result.ScheduleState = ScheduleStateValue.Parse (bytes [3]);

            return result;
        }

        public byte [] GetValueBytes ()
        {
            return new byte [] {
                (byte)OverrideType,
                ScheduleState.GetValueByte()
            };
        }

        public override string ToString ()
        {
            return string.Format ("[ClimateControlScheduleOverrideValue: OverrideType={0}, ScheduleState={1}]", OverrideType, ScheduleState);
        }
    }
}
