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
