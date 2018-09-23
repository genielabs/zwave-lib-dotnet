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
*     Author: https://github.com/mdave
*     Author: https://github.com/bounz
*     Project Homepage: https://github.com/genielabs/zwave-lib-dotnet
*/
using System;
using ZWaveLib.Values;

namespace ZWaveLib.CommandClasses
{
    public class Version : ICommandClass
    {
        public CommandClass GetClassId()
        {
            return CommandClass.Version;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            NodeEvent nodeEvent = null;
            var type = (Command)message[1];

            if (type == Command.VersionReport)
            {
                var nodeVersion = new NodeVersion {
                    LibraryType = message[2],
                    ProtocolVersion = message[3],
                    ProtocolSubVersion = message[4],
                    ApplicationVersion = message[5],
                    ApplicationSubVersion = message[6]
                };
                node.Version = nodeVersion;
                nodeEvent = new NodeEvent(node, EventParameter.VersionCommandClass, nodeVersion, 0);
            }

            if (type == Command.VersionCommandClassReport)
            {
                var cmdClass = (CommandClass)message[2];
                var value = new VersionValue(cmdClass, message[3]);
                // Update node CC data
                if (cmdClass != CommandClass.NotSet)
                {
                    var nodeCc = node.GetCommandClass(cmdClass);
                    if (nodeCc != null)
                        nodeCc.Version = value.Version;
                    // Set the VersionCommandClass event
                    nodeEvent = new NodeEvent(node, EventParameter.VersionCommandClass, value, 0);
                }
                else
                {
                    Utility.logger.Warn("Command Class {0} ({1}) not supported yet", message[3], message[3].ToString("X2"));
                }
            }

            return nodeEvent;
        }

        public static ZWaveMessage Get(ZWaveNode node, CommandClass cmdClass)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.Version, 
                (byte)Command.VersionCommandClassGet,
                (byte)cmdClass
            });
        }

        public static ZWaveMessage Report(ZWaveNode node)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.Version, 
                (byte)Command.VersionGet,
            });
        }
    }
}
