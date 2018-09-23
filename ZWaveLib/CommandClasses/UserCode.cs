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
using ZWaveLib.Values;
using System.Collections.Generic;

namespace ZWaveLib.CommandClasses
{
    public class UserCode : ICommandClass
    {
        public CommandClass GetClassId()
        {
            return CommandClass.UserCode;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            NodeEvent nodeEvent = null;
            byte cmdType = message[1];
            if (cmdType == (byte)Command.UserCodeReport)
            {
                var reportedUserCode = UserCodeValue.Parse(message);
                var userCode = GetUserCodeData(node);
                userCode.TagCode = reportedUserCode.TagCode;
                userCode.UserId = reportedUserCode.UserId;
                userCode.UserIdStatus = reportedUserCode.UserIdStatus;
                nodeEvent = new NodeEvent(node, EventParameter.UserCode, reportedUserCode, 0);
            }
            return nodeEvent;
        }

        public static ZWaveMessage Set(ZWaveNode node, UserCodeValue newUserCode)
        {
            var userCode = GetUserCodeData(node);
            userCode.TagCode = newUserCode.TagCode;
            userCode.UserId = newUserCode.UserId;
            userCode.UserIdStatus = newUserCode.UserIdStatus;
            List<byte> message = new List<byte>();
            message.Add((byte)CommandClass.UserCode);
            message.Add((byte)Command.UserCodeSet);
            message.Add(userCode.UserId);
            message.Add(userCode.UserIdStatus);
            message.AddRange(userCode.TagCode);
            return node.SendDataRequest(message.ToArray());
        }

        public static UserCodeValue GetUserCode(ZWaveNode node)
        {
            return GetUserCodeData(node);
        }

        private static UserCodeValue GetUserCodeData(ZWaveNode node)
        {
            return (UserCodeValue)node.GetData("UserCode", new UserCodeValue()).Value;
        }

    }
}