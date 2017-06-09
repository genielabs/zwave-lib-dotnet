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
    public class SwitchpointValue
    {

        public int Hour { get; set; }

        public int Minute { get; set; }

        public ScheduleStateValue State { get; set; }

        public byte [] GetValueBytes ()
        {
            return new byte [] {
                (byte)(Hour & 0x1f),
                (byte)(Minute & 0x3f),
                State.GetValueByte()
            };
        }

        public static SwitchpointValue Parse (byte [] bytes, int offset)
        {
            var switchpoint = new SwitchpointValue ();

            switchpoint.Hour = bytes [offset] & 0x1f;
            switchpoint.Minute = bytes [offset + 1] & 0x3f;
            switchpoint.State = ScheduleStateValue.Parse(bytes[offset + 2]);

            return switchpoint;
        }

        public override string ToString ()
        {
            return string.Format ("[SwitchpointValue: Hour={0}, Minute={1}, State={2}]", Hour, Minute, State);
        }
    }
}
