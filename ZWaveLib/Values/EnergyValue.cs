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
    public enum ZWaveEnergyScaleType : int
    {
        kWh = 0x00,
        kVAh = 0x01,
        Watt = 0x02,
        Pulses = 0x03,
        ACVolt = 0x04,
        ACCurrent = 0x05,
        PowerFactor = 0x06,
        Unknown = 0xFF
    }

    public class EnergyValue
    {
        public EventParameter EventType = EventParameter.SensorGeneric;
        public ZWaveEnergyScaleType Parameter = ZWaveEnergyScaleType.Unknown;
        public double Value = 0;

        public static EnergyValue Parse(byte[] message)
        {
            ZWaveValue zvalue = ZWaveValue.ExtractValueFromBytes(message, 4);
            //int meterType = (message[1] & 0x1f);
            //Utility.logger.Warn("METER TYPE ({0})!", meterType);
            EnergyValue energy = new EnergyValue();
            energy.Value = zvalue.Value;
            if (Enum.IsDefined(typeof(ZWaveEnergyScaleType), zvalue.Scale))
            {
                energy.Parameter = (ZWaveEnergyScaleType)zvalue.Scale;
            }
            else
            {
                Utility.logger.Warn("Undefined Energy Meter Scale Type {0}!", zvalue.Scale);
            }


            // sometimes it appears this odd extra bit 0x80 (128) in message[2] 
            // when scale type should be power factor it is interpreted as watt
            // (it happens with Qbino smart meter)
            // the following is a temporary work-around for that
            // TODO: find out the meaning of message[2] = 0xA1 (in place of 0x21)
            if ((message[2] & 0x80) == 0x80 && energy.Parameter == ZWaveEnergyScaleType.Watt)
                energy.Parameter = ZWaveEnergyScaleType.PowerFactor;


            switch (energy.Parameter)
            {
            // Accumulated power consumption kW/h
            case ZWaveEnergyScaleType.kWh:
                energy.EventType = EventParameter.MeterKwHour;
                break;
            // Accumulated power consumption kilo Volt Ampere / hours (kVA/h)
            case ZWaveEnergyScaleType.kVAh:
                energy.EventType = EventParameter.MeterKvaHour;
                break;
            // Instant power consumption Watt
            case ZWaveEnergyScaleType.Watt:
                energy.EventType = EventParameter.MeterWatt;
                break;
            // Pulses count
            case ZWaveEnergyScaleType.Pulses:
                energy.EventType = EventParameter.MeterPulses;
                break;
            // AC load Voltage
            case ZWaveEnergyScaleType.ACVolt:
                energy.EventType = EventParameter.MeterAcVolt;
                break;
            // AC load Current
            case ZWaveEnergyScaleType.ACCurrent:
                energy.EventType = EventParameter.MeterAcCurrent;
                break;
            // Power Factor
            case ZWaveEnergyScaleType.PowerFactor:
                energy.EventType = EventParameter.MeterPower;
                break;
            default:
                // Unknown value ?
                energy = null;
                break;
            }
            return energy;
        }
    }
}

