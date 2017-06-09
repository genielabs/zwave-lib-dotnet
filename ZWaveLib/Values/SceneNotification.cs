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

    public enum KeyAttributes : byte
    {
        KeyPressed = 0,
        KeyReleased = 1,
        KeyHeldDown = 2,
    }

    public class SceneNotification
    {
        public byte SequenceNumber;
        public KeyAttributes KeyAttributes;
        public byte SceneNumber;

        public static SceneNotification Parse (byte [] message)
        {
            var value = new SceneNotification ();

            value.SequenceNumber = message [2];
            value.KeyAttributes = (KeyAttributes)(message [3] & 0x07);
            value.SceneNumber = message [4];

            return value;
        }

    }
}
