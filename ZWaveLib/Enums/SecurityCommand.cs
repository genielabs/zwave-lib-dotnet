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
 *     Author: https://github.com/snagytx
 *     Project Homepage: https://github.com/genielabs/zwave-lib-dotnet
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZWaveLib
{
    public enum SecurityCommand
    {
        SupportedGet = 0x02,
        SupportedReport = 0x03,
        SchemeGet = 0x04,
        SchemeReport = 0x05,
        NetworkKeySet = 0x06,
        NetworkKeyVerify = 0x07,
        SchemeInherit = 0x08,
        NonceGet = 0x40,
        NonceReport = 0x80,
        MessageEncap = 0x81,
        MessageEncapNonceGet = 0xc1
    }


    public enum SecurityScheme : byte
    {
        SchemeZero = 0x00,
        SchemeOne = 0x01,
        SchemeReserved1 = 0x02,
        SchemeReserved2 = 0x04,
        SchemeReserved3 = 0x08,
        SchemeReserved4 = 0x10,
        SchemeReserved5 = 0x20,
        SchemeReserved6 = 0x40,
        SchemeReserved7 = 0x80
    }
}
