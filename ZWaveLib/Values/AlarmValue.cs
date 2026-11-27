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

        internal bool IsTypeIdle { get; private set; }

        public static AlarmValue Parse(byte[] message)
        {
            AlarmValue alarm = new AlarmValue();
            if (message == null || message.Length < 4)
                return null;
            bool idle = false;
            byte cmdClass = message[0];

            if (cmdClass == (byte)CommandClass.SensorAlarm)
            {
                alarm.Parameter = (ZWaveAlarmType)Enum.Parse(typeof(ZWaveAlarmType), message[3].ToString());
                alarm.Value = message[4];
            }
            else // CommandClass.Alarm
            {
                if (message.Length > 4 && message.Length < 8)
                    return null;
                if (message.Length >= 8 && message[6] != 0)
                {
                    int properties = message.Length >= 9 ? message[8] : 0;
                    int parameterLength = properties & 0x1f;
                    int sequenceLength = (properties & 0x80) == 0 ? 0 : 1;
                    if (message.Length >= 9 && message.Length < 9 + parameterLength + sequenceLength)
                        return null;
                    idle = message[7] == 0;
                    alarm.IsTypeIdle = idle && parameterLength == 0;
                    alarm.Event = (ZWaveAlarmEvent)(idle && parameterLength > 0 ? message[9] : message[7]);
                    alarm.Parameter = (ZWaveAlarmType)message[6];
                    // Notification Status describes reporting, not the sensor state.
                    alarm.Value = idle ? (byte)0 : (byte)0xff;
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

            if (idle)
                alarm.Value = 0;
            return alarm;
        }
    }
}
