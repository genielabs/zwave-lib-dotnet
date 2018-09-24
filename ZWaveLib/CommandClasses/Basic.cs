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

namespace ZWaveLib.CommandClasses
{
    public class Basic : ICommandClass
    {
        public CommandClass GetClassId()
        {
            return CommandClass.Basic;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            NodeEvent nodeEvent = null;
            byte cmdType = message[1];
            if (cmdType == (byte)Command.BasicReport || cmdType == (byte)Command.BasicSet)
            {
                int levelValue = (int)message[2];
                nodeEvent = new NodeEvent(node, EventParameter.Basic, (double)levelValue, 0);
            }
            return nodeEvent;
        }

        public static ZWaveMessage Set(ZWaveNode node, int value)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.Basic, 
                (byte)Command.BasicSet, 
                byte.Parse(value.ToString())
            });
        }

        public static ZWaveMessage Get(ZWaveNode node)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.Basic, 
                (byte)Command.BasicGet 
            });
        }

    }
}

