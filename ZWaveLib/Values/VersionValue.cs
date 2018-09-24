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
*     Author: http://github.com/mdave
*     Project Homepage: https://github.com/genielabs/zwave-lib-dotnet
*/

using System;
using System.Collections;

namespace ZWaveLib.Values
{
    public class VersionValue
    {
        public readonly CommandClass CmdClass;
        public readonly int Version;

        public VersionValue(CommandClass cmdClass, int version)
        {
            CmdClass = cmdClass;
            Version = version;
        }

        public VersionValue()
        {
            CmdClass = 0;
            Version = 0;
        }
    }
}