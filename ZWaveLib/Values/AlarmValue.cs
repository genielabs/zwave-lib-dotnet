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
*     Project Homepage: https://github.com/genielabs/zwave-lib-dotnet
*/

using System;

namespace ZWaveLib.Values
{
    public enum ZWaveAlarmType
    {
        Generic = 0,
        Smoke,
        CarbonMonoxide,
        CarbonDioxide,
        Heat,
        Flood,
        AccessControl,
        Burglar,
        PowerManagement,
        System,
        Emergency,
        Clock,
        Appliance,
        HomeHealth
    }

    /// <summary>
    /// Enumerator for alarm value details. e.g., 0x16 corresponds with the action of 
    /// an open door if the alarm AccessControl alarm type is set.
    /// </summary>
    /// 
    public enum ZWaveAlarmEvent
    {
        Generic = 0x00,
        // Home security
        HomeSecurityTamper = 0x03,
        HomeSecurityMotion = 0x07, // Air?
        HomeSecurityPir = 0x08,
        // Access control
        AccessDoorOpen = 0x16,
        AccessDoorClosed = 0x17,
    }

    public class AlarmValue
    {
        public EventParameter EventType = EventParameter.AlarmGeneric;
        public ZWaveAlarmType Parameter = ZWaveAlarmType.Generic;
        public ZWaveAlarmEvent Event = ZWaveAlarmEvent.Generic;
        public byte Value = 0x00;

        public static AlarmValue Parse(byte[] message)
        {
            AlarmValue alarm = new AlarmValue();
            byte cmdClass = message[0];

            if (cmdClass == (byte)CommandClass.SensorAlarm)
            {
                alarm.Parameter = (ZWaveAlarmType)Enum.Parse(typeof(ZWaveAlarmType), message[3].ToString());
                alarm.Value = message[4];
            }
            else // CommandClass.Alarm
            {
                // TODO: change this to --> if (node.GetCommandClass(CommandClass.Alarm).Version == 2) ...
                if (message.Length > 7)
                {
                    // Version 2
                    alarm.Event = (ZWaveAlarmEvent)Enum.Parse(typeof(ZWaveAlarmEvent), message[7].ToString());
                    alarm.Parameter = (ZWaveAlarmType)Enum.Parse(typeof(ZWaveAlarmType), message[6].ToString());
                    alarm.Value = message[5];
                }
                else
                {
                    // Version 1
                    alarm.Parameter = (ZWaveAlarmType)Enum.Parse(typeof(ZWaveAlarmType), message[2].ToString());
                    alarm.Value = message[3];
                }
            }

            switch (alarm.Parameter)
            {
            case ZWaveAlarmType.CarbonDioxide:
                alarm.EventType = EventParameter.AlarmCarbonDioxide;
                break;
            case ZWaveAlarmType.CarbonMonoxide:
                alarm.EventType = EventParameter.AlarmCarbonMonoxide;
                break;
            case ZWaveAlarmType.Smoke:
                alarm.EventType = EventParameter.AlarmSmoke;
                break;
            case ZWaveAlarmType.Heat:
                alarm.EventType = EventParameter.AlarmHeat;
                break;
            case ZWaveAlarmType.Flood:
                alarm.EventType = EventParameter.AlarmFlood;
                break;
            case ZWaveAlarmType.AccessControl:
                alarm.EventType = EventParameter.AlarmDoorWindow;
                switch (alarm.Event)
                {
                case ZWaveAlarmEvent.AccessDoorOpen:
                    alarm.Value = 1;
                    break;
                case ZWaveAlarmEvent.AccessDoorClosed:
                    alarm.Value = 0;
                    break;
                }
                break;
            case ZWaveAlarmType.Burglar:
                alarm.EventType = EventParameter.AlarmGeneric;
                switch (alarm.Event)
                {
                case ZWaveAlarmEvent.HomeSecurityTamper:
                    alarm.EventType = EventParameter.AlarmTampered;
                    break;
                case ZWaveAlarmEvent.HomeSecurityMotion:
                case ZWaveAlarmEvent.HomeSecurityPir:
                    alarm.EventType = EventParameter.SensorMotion;
                    break;
                }
                break;
            }

            return alarm;
        }
    }
}
