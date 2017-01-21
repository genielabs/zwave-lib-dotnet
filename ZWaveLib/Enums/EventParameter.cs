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

    public enum EventParameter
    {
        Basic,
        SwitchBinary,
        SwitchMultilevel,
        ManufacturerSpecific,
        MeterKwHour,
        MeterKvaHour,
        MeterWatt,
        MeterPulses,
        MeterAcVolt,
        MeterAcCurrent,
        MeterPower,
        SensorGeneric,
        SensorTemperature,
        SensorHumidity,
        SensorLuminance,
        SensorMotion,
        AlarmGeneric,
        AlarmDoorWindow,
        AlarmSmoke,
        AlarmCarbonMonoxide,
        AlarmCarbonDioxide,
        AlarmHeat,
        AlarmFlood,
        AlarmTampered,
        Configuration,
        WakeUpInterval,
        WakeUpNotify,
        WakeUpSleepingStatus,
        Association,
        VersionCommandClass,
        Battery,
        NodeInfo,
        MultiinstanceSwitchBinaryCount,
        MultiinstanceSwitchBinary,
        MultiinstanceSwitchMultilevelCount,
        MultiinstanceSwitchMultilevel,
        MultiinstanceSensorBinaryCount,
        MultiinstanceSensorBinary,
        MultiinstanceSensorMultilevelCount,
        MultiinstanceSensorMultilevel,
        ThermostatFanMode,
        ThermostatFanState,
        ThermostatHeating,
        ThermostatMode,
        ThermostatOperatingState,
        ThermostatSetBack,
        ThermostatSetPoint,
        UserCode,
        SecurityNodeInformationFrame,
        SecurityDecriptedMessage,
        SecurityGeneratedKey,
        DoorLockStatus,
        RoutingInfo,
        Clock
    }

}

