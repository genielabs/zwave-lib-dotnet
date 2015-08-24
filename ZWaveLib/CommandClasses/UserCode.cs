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