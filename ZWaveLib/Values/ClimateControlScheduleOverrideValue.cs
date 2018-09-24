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
