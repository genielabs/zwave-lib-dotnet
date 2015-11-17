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

    public enum ControllerStatus : byte
    {
        Connected,
        Disconnected,
        Initializing,
        Ready,
        Error
    }

    public enum CallbackStatus : byte
    {
        Ack = 0x00,
        Nack = 0x01,

        NeighborUpdateStarted = 0x21,
        NeighborUpdateDone = 0x22,

        NotSet = 0xFF
    }

    public enum DiscoveryStatus : uint
    {
        DiscoveryStart = 0x0201,
        DiscoveryEnd = 0x0202,
        DiscoveryError = 0x0203
    }

    public enum HealStatus : uint
    {
        HealStart = 0x0201,
        HealEnd = 0x0202,
        HealError = 0x0203
    }

    public enum NodeQueryStatus : uint
    {
        NodeAdded = 0xFF01,
        NodeRemoved = 0xFF02,
        NodeUpdated = 0xFF03,

        NodeAddReady = 0x4A00,
        NodeAddStarted = 0x4A01,
        NodeAddDone = 0x4A02,
        NodeAddFailed = 0x4A04,

        NodeRemoveReady = 0x4B00,
        NodeRemoveStarted = 0x4B01,
        NodeRemoveDone = 0x4B02,
        NodeRemoveFailed = 0x4B04,

        NeighborUpdateStarted = 0x5A01,
        NeighborUpdateDone = 0x5A02,
        NeighborUpdateFailed = 0x5A03,

        Error = 0xFFEE,
        Timeout = 0xFFFF
    }

    public enum NeighborsUpdateStatus : byte
    {
        None = 0x00,
        NeighborsUpdateStarted = 0x21,
        NeighborsUpdateDone = 0x22,
        NeighborsUpdateFailed = 0x23
    }

}

