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
using ZWaveLib.Values;

namespace ZWaveLib.CommandClasses
{
    /// <summary>
    /// Enumerator for possible sensor binary parameters (only reported for v2)
    /// </summary>
    public enum ZWaveSensorBinaryParameter : byte
    {
        Unknown = 0x00,
        General = 0x01,
        Smoke = 0x02,
        CarbonMonoxide = 0x03,
        CarbonDioxide = 0x04,
        Heat = 0x05,
        Water = 0x06,
        Freeze = 0x07,
        Tamper = 0x08,
        Auxiliary = 0x09,
        DoorWindow = 0x0a,
        Tilt = 0x0b,
        Motion = 0x0c,
        GlassBreak = 0x0d
    }

    public class SensorBinary : ICommandClass
    {
        public CommandClass GetClassId()
        {
            return CommandClass.SensorBinary;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            NodeEvent nodeEvent = null;
            byte cmdType = message[1];
            if (cmdType == (byte)Command.SensorBinaryReport)
            {
                byte version = node.GetCmdClassVersion(GetClassId());

                if (version == 1 || message.Length <= 3)
                {
                    nodeEvent = new NodeEvent(node, EventParameter.SensorGeneric, message[2], 0);
                }
                else
                {
                    byte tmp = message[3];
                    ZWaveSensorBinaryParameter sensorType = ZWaveSensorBinaryParameter.General;
                    EventParameter eventType;

                    if (Enum.IsDefined(typeof(ZWaveSensorBinaryParameter), tmp))
                    {
                        sensorType = (ZWaveSensorBinaryParameter)tmp;
                    }

                    switch (sensorType)
                    {
                    case ZWaveSensorBinaryParameter.Smoke:
                        eventType = EventParameter.AlarmSmoke;
                        break;
                    case ZWaveSensorBinaryParameter.CarbonMonoxide:
                        eventType = EventParameter.AlarmCarbonMonoxide;
                        break;
                    case ZWaveSensorBinaryParameter.CarbonDioxide:
                        eventType = EventParameter.AlarmCarbonDioxide;
                        break;
                    case ZWaveSensorBinaryParameter.Heat:
                        eventType = EventParameter.AlarmHeat;
                        break;
                    case ZWaveSensorBinaryParameter.Water:
                        eventType = EventParameter.AlarmFlood;
                        break;
                    case ZWaveSensorBinaryParameter.Tamper:
                        eventType = EventParameter.AlarmTampered;
                        break;
                    case ZWaveSensorBinaryParameter.DoorWindow:
                        eventType = EventParameter.AlarmDoorWindow;
                        break;
                    case ZWaveSensorBinaryParameter.Motion:
                        eventType = EventParameter.SensorMotion;
                        break;
                    case ZWaveSensorBinaryParameter.Freeze:
                    case ZWaveSensorBinaryParameter.Auxiliary:
                    case ZWaveSensorBinaryParameter.Tilt:
                    default:
                        // Catch-all for the undefined types above.
                        eventType = EventParameter.SensorGeneric;
                        break;
                    }

                    nodeEvent = new NodeEvent(node, eventType, message[2], 0);
                }
            }
            return nodeEvent;
        }

        public static void Get(ZWaveNode node)
        {
            node.SendDataRequest(new byte[] { 
                (byte)CommandClass.SensorBinary, 
                (byte)Command.SensorBinaryGet 
            });
        }
    }
}



