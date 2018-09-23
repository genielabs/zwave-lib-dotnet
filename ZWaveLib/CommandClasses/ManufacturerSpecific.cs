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

namespace ZWaveLib.CommandClasses
{
    
    public class ManufacturerSpecificInfo
    {
        public string ManufacturerId { get; set; }

        public string TypeId { get; set; }

        public string ProductId { get; set; }
    }

    public class ManufacturerSpecific : ICommandClass
    {
        public CommandClass GetClassId()
        {
            return CommandClass.ManufacturerSpecific;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            NodeEvent nodeEvent = null;

            if (message.Length > 7)
            {
                byte[] manufacturerId = new byte[2] { message[2], message[3] };
                byte[] typeId = new byte[2] { message[4], message[5] };
                byte[] productId = new byte[2] { message[6], message[7] };

                var manufacturerSpecs = new ManufacturerSpecificInfo() {
                    TypeId = BitConverter.ToString(typeId).Replace("-", ""),
                    ProductId = BitConverter.ToString(productId).Replace("-", ""),
                    ManufacturerId = BitConverter.ToString(manufacturerId).Replace("-", "")
                };
                node.ManufacturerSpecific.ManufacturerId = manufacturerSpecs.ManufacturerId;
                node.ManufacturerSpecific.TypeId = manufacturerSpecs.TypeId;
                node.ManufacturerSpecific.ProductId = manufacturerSpecs.ProductId;
                nodeEvent = new NodeEvent(node, EventParameter.ManufacturerSpecific, manufacturerSpecs, 0);
            }

            return nodeEvent;
        }

        public static ZWaveMessage Get(ZWaveNode node)
        {
            byte[] request = new byte[] {
                (byte)CommandClass.ManufacturerSpecific,
                (byte)Command.ManufacturerSpecificGet
            }; 
            return node.SendDataRequest(request);
        }

    }
}

