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
    public class Association : ICommandClass
    {
        public class AssociationResponse
        {
            public byte Max = 0;
            public byte Count = 0;
            public byte GroupId = 0;
            public string NodeList = "";
        }

        public CommandClass GetClassId()
        {
            return CommandClass.Association;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            NodeEvent nodeEvent = null;
            byte cmdType = message[1];
            
            // we want to get in to that we can handle NO Associations
            if (message.Length > 4 && cmdType == (byte)Command.AssociationReport)
            {
                byte groupId = message[2];
                byte associationMax = message[3];
                byte associationCount = message[4]; // it is always zero ?!?
                string associationNodes = "";
                if (message.Length > 4)
                {
                    for (int a = 5; a < message.Length; a++)
                    {
                        associationNodes += message[a] + ",";
                    }
                }
                associationNodes = associationNodes.TrimEnd(',');

                // We don't want to send empty response since it will be handled as "timeout"
                // so setting it to "None"
                if (associationNodes.Length == 0)
                {
                    associationNodes = "None";
                }
                //
                var associationResponse = new AssociationResponse() {
                    Max = associationMax,
                    Count = associationCount,
                    NodeList = associationNodes,
                    GroupId = groupId
                };
                nodeEvent = new NodeEvent(node, EventParameter.Association, associationResponse, 0);
            }

            return nodeEvent;
        }

        public static ZWaveMessage Set(ZWaveNode node, byte groupid, byte targetNodeId)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.Association, 
                (byte)Command.AssociationSet, 
                groupid, 
                targetNodeId 
            });
        }

        public static ZWaveMessage Get(ZWaveNode node, byte groupId)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.Association, 
                (byte)Command.AssociationGet, 
                groupId 
            });
        }

        public static ZWaveMessage Remove(ZWaveNode node, byte groupId, byte targetNodeId)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.Association, 
                (byte)Command.AssociationRemove, 
                groupId, 
                targetNodeId 
            });
        }
    }
}

