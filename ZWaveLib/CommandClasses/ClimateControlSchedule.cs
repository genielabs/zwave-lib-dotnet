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
 *             Ben Voss
 *     Project Homepage: https://github.com/genielabs/zwave-lib-dotnet
 */

using System.Collections.Generic;

namespace ZWaveLib.CommandClasses
{
    public class ClimateControlSchedule : ICommandClass
    {
        public CommandClass GetClassId ()
        {
            return CommandClass.ClimateControlSchedule;
        }

        public NodeEvent GetEvent (ZWaveNode node, byte [] message)
        {
            if (message.Length == 0) {
                return null;
            }

            byte cmdType = message [1];

            if (cmdType == (byte)Command.ScheduleReport) {
                var climateControlScheduleValue = ClimateControlScheduleValue.Parse (message);
                return new NodeEvent (node, EventParameter.ClimateControlSchedule, climateControlScheduleValue, 0);
            }

            if (cmdType == (byte)Command.ScheduleChangedReport) {
                return new NodeEvent (node, EventParameter.ClimateControlScheduleChanged, message[2], 0);
            }

            if (cmdType == (byte)Command.ScheduleOverrideReport) {
                var climateControlScheduleOverrideValue = ClimateControlScheduleOverrideValue.Parse (message);
                return new NodeEvent (node, EventParameter.ClimateControlScheduleOverride, climateControlScheduleOverrideValue, 0);
            }

            return null;
        }

        public static ZWaveMessage Set (ZWaveNode node, ClimateControlScheduleValue value)
        {
            var message = new List<byte> ();
            message.AddRange (new byte [] {
                (byte)CommandClass.ClimateControlSchedule,
                (byte)Command.ScheduleSet
            });
            message.AddRange (value.GetValueBytes ());

            return node.SendDataRequest (message.ToArray ());
        }

        public static ZWaveMessage Get (ZWaveNode node, Weekday weekday)
        {
            return node.SendDataRequest (new byte [] {
                (byte)CommandClass.ClimateControlSchedule,
                (byte)Command.ScheduleGet,
                (byte)weekday
            });
        }

        public static ZWaveMessage ChangedGet (ZWaveNode node)
        {
            return node.SendDataRequest (new byte [] {
                (byte)CommandClass.ClimateControlSchedule,
                (byte)Command.ScheduleChangedGet
            });
        }

        public static ZWaveMessage OverrideSet (ZWaveNode node, ClimateControlScheduleOverrideValue scheduleOverride)
        {
            var message = new List<byte> ();
            message.AddRange (new byte [] {
                (byte)CommandClass.ClimateControlSchedule,
                (byte)Command.ScheduleOverrideSet,
            });
            message.AddRange (scheduleOverride.GetValueBytes ());

            return node.SendDataRequest (message.ToArray ());
        }

        public static ZWaveMessage OverrideGet (ZWaveNode node)
        {
            return node.SendDataRequest (new byte [] {
                (byte)CommandClass.ClimateControlSchedule,
                (byte)Command.ScheduleOverrideGet
            });
        }
    }
}
