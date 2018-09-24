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
