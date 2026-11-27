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
    public class Alarm : ICommandClass
    {
        public CommandClass GetClassId()
        {
            return CommandClass.Alarm;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            if (message == null || message.Length < 2)
                return null;
            if (message[1] == (byte)Command.NotificationSupportedReport)
            {
                if (message.Length < 3 || (message[2] & 0x1f) == 0)
                    return null;
                var types = NotificationSupportedReport.ReadMask(message, 3, message[2] & 0x1f);
                if (types == null)
                    return null;
                var report = new NotificationSupportedReport { Types = types, SupportsLegacyAlarm = (message[2] & 0x80) != 0 };
                return new NodeEvent(node, EventParameter.NotificationSupportedReport, report, 0);
            }
            if (message[1] == (byte)Command.NotificationEventSupportedReport)
            {
                if (message.Length < 4)
                    return null;
                var events = NotificationSupportedReport.ReadMask(message, 4, message[3] & 0x1f);
                if (events == null)
                    return null;
                var report = new NotificationEventSupportedReport { Type = message[2], Events = events };
                return new NodeEvent(node, EventParameter.NotificationEventSupportedReport, report, 0);
            }
            NodeEvent nodeEvent = null;
            byte cmdType = message[1];
            if (cmdType == (byte)Command.AlarmReport)
            {
                var alarm = AlarmValue.Parse(message);
                if (alarm == null)
                    return null;
                nodeEvent = new NodeEvent(node, alarm.EventType, alarm.Value, 0);
                // Without an event parameter, idle applies to every state of this type.
                if (alarm.Parameter == ZWaveAlarmType.Burglar && alarm.IsTypeIdle)
                {
                    nodeEvent.NestedEvent = new NodeEvent(node, EventParameter.SensorMotion, (byte)0, 0);
                    nodeEvent.NestedEvent.NestedEvent = new NodeEvent(node, EventParameter.AlarmTampered, (byte)0, 0);
                }
            }
            return nodeEvent;
        }

        public static ZWaveMessage GetSupported(ZWaveNode node)
        {
            return node.SendDataRequest(new[] { (byte)CommandClass.Alarm, (byte)Command.NotificationSupportedGet });
        }

        public static ZWaveMessage GetSupportedEvents(ZWaveNode node, byte type)
        {
            return node.SendDataRequest(new[] { (byte)CommandClass.Alarm, (byte)Command.NotificationEventSupportedGet, type });
        }
    
    }
}

