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
using System.Linq;

namespace ZWaveLib.Values
{
    public class CapabilityReport
    {
        public CapabilityReport(bool isDynamic, int endPoint, int genericDeviceClass, int specificDeviceClass, int[] commandClasses)
        {
            IsDynamic = isDynamic;
            EndPoint = endPoint;
            GenericDeviceClass = genericDeviceClass;
            SpecificDeviceClass = specificDeviceClass;
            CommandClasses = commandClasses;
        }

        public bool IsDynamic { get; }

        public int EndPoint { get; }

        public int GenericDeviceClass { get; }

        public int SpecificDeviceClass { get; }

        public int[] CommandClasses { get; }

        public override string ToString()
        {
            var commandClasses = String.Join(",", CommandClasses.Select(x => x.ToString("X2")).ToArray());

            return String.Format("CapabilityReport: IsDynamic={0}, EndPoint={1}, GenericDeviceClass={2}, SpecificDeviceClass={3}, CommandClasses={4}",
                                 IsDynamic, EndPoint, GenericDeviceClass, SpecificDeviceClass, commandClasses);
        }
    }
}
