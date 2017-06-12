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
