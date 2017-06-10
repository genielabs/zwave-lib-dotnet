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

using System.Text;
using System.Collections.Generic;

namespace ZWaveLib
{
    public class ClimateControlScheduleValue
    {
        private const int NumSwitchpoints = 9;

        public ClimateControlScheduleValue ()
        {
            Switchpoints = new SwitchpointValue [NumSwitchpoints];
            for (int i = 0; i < NumSwitchpoints; i++) {
                Switchpoints [i] = new SwitchpointValue { State = new ScheduleStateValue { Unused = true } };
            }
        }

        public Weekday Weekday { get; set; }

        public SwitchpointValue[] Switchpoints { get; }

        public byte [] GetValueBytes ()
        {
            var result = new List<byte> ();
            result.Add ((byte)Weekday);

            for (int i = 0; i < NumSwitchpoints; i++) {
                result.AddRange (Switchpoints[i].GetValueBytes ());
            }

            return result.ToArray();
        }

        public static ClimateControlScheduleValue Parse (byte [] bytes) {
            var result = new ClimateControlScheduleValue ();

            result.Weekday = (Weekday)bytes [2];

            for (int i = 0; i < NumSwitchpoints; i++) {
                result.Switchpoints [i] = SwitchpointValue.Parse(bytes, i * 3 + 3);

            }

            return result;
        }

        public override string ToString ()
        {
            var builder = new StringBuilder ();
            builder.Append("[ClimateControlScheduleValue: Weekday=");
            builder.Append (Weekday);
            builder.Append (", Switchpoints: [");

            for (int i = 0; i < NumSwitchpoints; i++) {
                builder.AppendLine().Append("   Switchpoint: ").Append(i).Append(" ").Append (Switchpoints [i]);
            }

            builder.Append ("]");

            return builder.ToString ();
        }
    }
}
