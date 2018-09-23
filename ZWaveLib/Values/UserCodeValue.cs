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
 *     Author: Alexandre Schnegg <alexandre.schnegg@gmail.com>
 *     Author: Generoso Martello <gene@homegenie.it>
 *     Project Homepage: https://github.com/genielabs/zwave-lib-dotnet
 */

using System;
using System.Collections;

namespace ZWaveLib.Values
{
    public class UserCodeValue
    {
        public byte UserId;
        public byte UserIdStatus;
        public byte[] TagCode = new byte[10];

        public UserCodeValue(byte userId, byte userIdStatus, byte[] tagCode)
        {
            this.UserId = userId;
            this.UserIdStatus = userIdStatus;
            tagCode.CopyTo(this.TagCode, 0);
        }

        public UserCodeValue()
        {
            UserId = 0;
            UserIdStatus = 0;
            TagCode = null;
        }

        public static UserCodeValue Parse(byte[] message)
        {
            UserCodeValue userCode = new UserCodeValue();
            userCode.UserId = message[2];
            userCode.UserIdStatus = message[3];
            userCode.TagCode = new byte[10];
            for (int i = 0; i < 10; i++)
            {
                userCode.TagCode[i] = message[4 + i];
            }
            return userCode;
        }

        public string TagCodeToHexString()
        {
            return Utility.ByteArrayToHexString(TagCode);
        }
    }
}