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

namespace ZWaveLib
{
    
    public enum GenericType : byte
    {
        None = 0x00,
        GenericController = 0x01,
        StaticController = 0x02,
        AvControlPoint = 0x03,
        RoutingSlave = 0x04, // this is a BasicType entry
        Display = 0x06,
        GarageDoor = 0x07,
        Thermostat = 0x08,
        WindowCovering = 0x09,
        RepeaterSlave = 0x0F,
        SwitchBinary = 0x10,
        SwitchMultilevel = 0x11,
        SwitchRemote = 0x12,
        SwitchToggle = 0x13,
        SensorBinary = 0x20,
        SensorMultilevel = 0x21,
        WaterControl = 0x22,
        MeterPulse = 0x30,
        Meter = 0x31,
        EntryControl = 0x40,
        SemiInteroperable = 0x50,
        SensorAlarm = 0xA1,
        NonInteroperable = 0xFF
    }

}

