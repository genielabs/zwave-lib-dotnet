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
    public class ThermostatOperatingState : ICommandClass
    {
        public enum Value
        {
            Idle = 0x00,
            Heating = 0x01,
            Cooling = 0x02,
            FanOnly = 0x03,
            PendingHeat = 0x04,
            PendingCool = 0x05,
            VentEconomizer = 0x06,
            State07 = 0x07,
            State08 = 0x08,
            State09 = 0x09,
            State10 = 0x0A,
            State11 = 0x0B,
            State12 = 0x0C,
            State13 = 0x0D,
            State14 = 0x0E,
            State15 = 0x0F
        }

        public CommandClass GetClassId()
        {
            return CommandClass.ThermostatOperatingState;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            return new NodeEvent(node, EventParameter.ThermostatOperatingState, message[2], 0);
        }

        public static void GetOperatingState(ZWaveNode node)
        {
            node.SendDataRequest(new byte[] { 
                (byte)CommandClass.ThermostatOperatingState, 
                (byte)Command.BasicGet
            });
        }
    }
}
