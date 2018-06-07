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

namespace ZWaveLib.CommandClasses
{

    public class ThermostatMode : ICommandClass
    {
        public enum Value
        {
            Off = 0x00,
            Heat = 0x01,
            Cool = 0x02,
            Auto = 0x03,
            AuxHeat = 0x04,
            Resume = 0x05,
            FanOnly = 0x06,
            Furnace = 0x07,
            DryAir = 0x08,
            MoistAir = 0x09,
            AutoChangeover = 0x0A,
            HeatEconomy = 0x0B,
            CoolEconomy = 0x0C,
            Away = 0x0D
        }

        public CommandClass GetClassId()
        {
            return CommandClass.ThermostatMode;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            return  new NodeEvent(node, EventParameter.ThermostatMode, (Value)message[2], 0);
        }

        public static ZWaveMessage Get(ZWaveNode node)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.ThermostatMode, 
                (byte)Command.BasicGet
            });
        }

        public static ZWaveMessage Set(ZWaveNode node, Value mode)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.ThermostatMode, 
                (byte)Command.BasicSet, 
                (byte)mode
            });
        }

        public static ZWaveMessage Set (ZWaveNode node, int channel, Value mode)
        {
            return node.SendDataRequest (new byte [] {
                (byte)CommandClass.MultiInstance,
                (byte)Command.MultiChannelEncapsulated,
                0,
                (byte)channel,
                (byte)CommandClass.ThermostatMode,
                (byte)Command.BasicSet,
                (byte)mode
            });

        }
    }
}
