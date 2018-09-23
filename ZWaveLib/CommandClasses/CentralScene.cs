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
 *     Author: D�lano Reijnierse (bluewalk)
 *     Project Homepage: https://github.com/genielabs/zwave-lib-dotnet
 */

using System;
using ZWaveLib.Values;

namespace ZWaveLib.CommandClasses
{
    public class CentralScene : ICommandClass
    {
        public CommandClass GetClassId()
        {
            return CommandClass.CentralScene;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            NodeEvent nodeEvent = null;

            switch ((Command)message[1])
            {
                case Command.CentralSceneNotification:
                    var value = CentralSceneValue.Parse(message);
                    nodeEvent = new NodeEvent(node, EventParameter.CentralSceneNotification, value, 0);
                    break;

                case Command.CentralSceneSupportedReport:
                    nodeEvent = new NodeEvent(node, EventParameter.CentralSceneSupportedReport, (int)message[2], 0);
                    break;
            }

            return nodeEvent;
        }

        public static ZWaveMessage SupportedGet(ZWaveNode node)
        {
            return node.SendDataRequest(new byte[] {
                (byte)CommandClass.CentralScene,
                (byte)Command.CentralSceneSupportedGet
            });
        }
    }
}