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
*     Author: http://github.com/mdave
*     Project Homepage: https://github.com/genielabs/zwave-lib-dotnet
*/

using System;
using System.Collections;

namespace ZWaveLib.Values
{
    public class VersionCmdClassValue
    {
        public CommandClass cmdClass;
        public byte version;

        public VersionCmdClassValue(CommandClass cmdClass, byte version)
        {
            this.cmdClass = cmdClass;
            this.version = version;
        }

        public VersionCmdClassValue()
        {
            this.cmdClass = 0;
            this.version = 0;
        }
    }
}