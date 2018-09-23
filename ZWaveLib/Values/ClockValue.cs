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

using System;

namespace ZWaveLib
{
    public enum Weekday
    {
        None = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6,
        Sunday = 7
    }

    public class ClockValue
    {
        public Weekday Weekday { get; set; }
        public int Hour { get; set;}
        public int Minute { get; set; }

        public static ClockValue Parse (byte [] message)
        {
            var value = new ClockValue ();

            value.Weekday = (Weekday)(message [2] >> 5);
            value.Hour = message [2] & 0x1f;
            value.Minute = message [3];

            return value;
        }

        public byte [] GetValueBytes ()
        {
            return new byte [] { (byte)((int)Weekday << 5 | Hour), (byte)Minute };
        }

        public override string ToString ()
        {
            return string.Format ("[ClockValue: Weekday={0}, Hour={1}, Minute={2}]", Weekday, Hour, Minute);
        }
    }
}
