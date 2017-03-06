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
    }
}
