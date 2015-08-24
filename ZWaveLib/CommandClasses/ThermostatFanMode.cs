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

    public class ThermostatFanMode : ICommandClass
    {
        public enum Value
        {
            AutoLow = 0x00,
            OnLow = 0x01,
            AutoHigh = 0x02,
            OnHigh = 0x03,
            Unknown4 = 0x04,
            Unknown5 = 0x05,
            Circulate = 0x06
        }

        public CommandClass GetClassId()
        {
            return CommandClass.ThermostatFanMode;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            return new NodeEvent(node, EventParameter.ThermostatFanMode, message[2], 0);
        }

        public static ZWaveMessage Get(ZWaveNode node)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.ThermostatFanMode, 
                (byte)Command.BasicGet
            });
        }

        public static ZWaveMessage Set(ZWaveNode node, Value mode)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.ThermostatFanMode, 
                (byte)Command.BasicSet, 
                (byte)mode
            });
        }
    }

}
