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

using ZWaveLib.Values;

namespace ZWaveLib.CommandClasses
{
    public class Meter : ICommandClass
    {
        public CommandClass GetClassId()
        {
            return CommandClass.Meter;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            if (message == null || message.Length < 2)
                return null;
            if (message[1] == (byte)Command.MeterSupportedReport)
            {
                int version = node.GetCommandClass(CommandClass.Meter)?.Version ?? -1;
                var report = MeterSupportedReport.Parse(message, version);
                return report == null ? null : new NodeEvent(node, EventParameter.MeterSupportedReport, report, 0);
            }
            NodeEvent nodeEvent = null;
            byte cmdType = message[1];
            if (cmdType == (byte)Command.MeterReport)
            {
                if (message.Length < 4)
                    return null;
                int size = message[3] & 7;
                if ((size != 1 && size != 2 && size != 4) || message.Length < 4 + size)
                    return null;
                EnergyValue energy = EnergyValue.Parse(message);
                if (energy != null)
                    nodeEvent = new NodeEvent(node, energy.EventType, energy.Value, 0);
            }
            return nodeEvent;
        }

        public static ZWaveMessage Get(ZWaveNode node, byte scaleType)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.Meter, 
                (byte)Command.MeterGet,
                scaleType
            });
        }

        public static ZWaveMessage GetSupported(ZWaveNode node)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.Meter, 
                (byte)Command.MeterSupportedGet
            });
        }

        public static ZWaveMessage Reset(ZWaveNode node)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.Meter, 
                (byte)Command.MeterReset
            });
        }
    }
}

