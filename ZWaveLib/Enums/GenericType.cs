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

