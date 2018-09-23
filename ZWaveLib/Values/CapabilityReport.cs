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
