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
 *     Project Homepage: https://github.com/genielabs/zwave-lib-dotnet
 */

using System;
using System.Collections.Generic;
using System.Threading;

namespace ZWaveLib.CommandClasses
{
    public class Configuration : ICommandClass
    {
        public CommandClass GetClassId()
        {
            return CommandClass.Configuration;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            NodeEvent nodeEvent = null;
            byte cmdType = message[1];
            if (message.Length > 4 && cmdType == (byte)Command.ConfigurationReport)
            {
                byte paramId = message[2];
                byte paramLength = message[3];
                //
                var nodeConfigParamsLength = GetConfigParamsData(node);
                if (!nodeConfigParamsLength.ContainsKey(paramId))
                {
                    nodeConfigParamsLength.Add(paramId, paramLength);
                }
                else
                {
                    // this shouldn't change on read... but you never know! =)
                    nodeConfigParamsLength[paramId] = paramLength;
                }
                //
                byte[] bval = new byte[4];
                // extract bytes value
                Array.Copy(message, 4, bval, 4 - (int)paramLength, (int)paramLength);
                uint paramValue = bval[0];
                Array.Reverse(bval);
                // convert it to uint
                paramValue = BitConverter.ToUInt32(bval, 0);
                nodeEvent = new NodeEvent(node, EventParameter.Configuration, paramValue, paramId);
            }
            return nodeEvent;
        }

        public static ZWaveMessage Set(ZWaveNode node, byte parameter, Int32 paramValue)
        {
            int valueLength = 1;
            var nodeConfigParamsLength = GetConfigParamsData(node);
            if (!nodeConfigParamsLength.ContainsKey(parameter))
            {
                Get(node, parameter);
                int retries = 0;
                // TODO: check if this can be removed by using the .Wait method 
                // TODO: in the "Get(node, parameter)" instruction above
                while (!nodeConfigParamsLength.ContainsKey(parameter) && retries++ <= 5)
                {
                    Thread.Sleep(1000);
                }
            }
            if (nodeConfigParamsLength.ContainsKey(parameter))
            {
                valueLength = nodeConfigParamsLength[parameter];
            }
            //
            byte[] value32 = BitConverter.GetBytes(paramValue);
            Array.Reverse(value32);
            //
            byte[] msg = new byte[4 + valueLength];
            msg[0] = (byte)CommandClass.Configuration;
            msg[1] = (byte)Command.ConfigurationSet;
            msg[2] = parameter;
            msg[3] = (byte)valueLength;
            switch (valueLength)
            {
            case 1:
                Array.Copy(value32, 3, msg, 4, 1);
                break;
            case 2:
                Array.Copy(value32, 2, msg, 4, 2);
                break;
            case 4:
                Array.Copy(value32, 0, msg, 4, 4);
                break;
            }
            return node.SendDataRequest(msg);
        }

        public static ZWaveMessage Get(ZWaveNode node, byte parameter)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.Configuration, 
                (byte)Command.ConfigurationGet,
                parameter
            });
        }

        private static Dictionary<byte, int> GetConfigParamsData(ZWaveNode node)
        {
            return (Dictionary<byte, int>)node.GetData("ConfigParamsLength", new Dictionary<byte, int>()).Value;
        }

    }
}

