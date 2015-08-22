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
    public class ThermostatSetBack : ICommandClass
    {
        public CommandClass GetClassId()
        {
            return CommandClass.ThermostatSetBack;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            return new NodeEvent(node, EventParameter.ThermostatSetBack, message[2], 0);
        }
    }
}
