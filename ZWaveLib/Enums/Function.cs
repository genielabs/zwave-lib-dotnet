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

    public enum ZWaveFunction: byte
    {
        NotSet = 0x00,

        GetInitData = 0x02,
        GetControllerCapabilities = 0x07,
        GetControllerInfo = 0x015,
        GetHomeId = 0x20,
        GetSucNodeId = 0x56,

        ApplicationCommandHandler = 0x04,
        ControllerSoftReset = 0x08,
        SendData = 0x13,

        GetNodeProtocolInfo = 0x41,
        ControllerSetDefault = 0x42,

        RequestNodeNeighborsUpdate = 0x48,

        ApplicationUpdate = 0x49,
        NodeAdd = 0x4A,
        NodeRemove = 0x4B,

        RequestNodeNeighborsUpdateOptions = 0x5A,

        RequestNodeInfo = 0x60,

        GetRoutingInfo = 0x80
    }

    public enum NodeFunctionOption : byte
    {
        AddNodeAny = 0x01,
        AddNodeController = 0x02,
        AddNodeSlave = 0x03,
        AddNodeExisting = 0x04,
        AddNodeStop = 0x05,
        //
        RemoveNodeAny = 0x01,
        RemoveNodeController = 0x02,
        RemoveNodeSlave = 0x03,
        RemoveNodeStop = 0x05
    }

    public enum NodeAddStatus : byte
    {
        None = 0x00,
        LearnReady = 0x01,
        NodeFound = 0x02,
        AddingSlave = 0x03,
        AddingController = 0x04,
        ProtocolDone = 0x05, // <--- DOES THIS REALLY EXISTS?
        Done = 0x06,
        Failed = 0x07,
    }

    public enum NodeRemoveStatus : byte
    {
        None = 0x00,
        LearnReady = 0x01,
        NodeFound = 0x02,
        RemovingSlave = 0x03,
        RemovingController = 0x04,
        Done = 0x06,
        Failed = 0x07
    }

    public enum NodeInfoStatus : byte
    {
        None = 0x00,
        NodeInfoReady = 0x01
    }

}

