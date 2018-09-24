/*
  This file is part of ZWaveLib (https://github.com/genielabs/zwave-lib-dotnet)

  Copyright (2012-2018) G-Labs (https://github.com/genielabs)

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
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

    public enum ApplicationUpdateStatus : byte
    {
        None = 0x00,
        RequestNodeInfoFailed = 0x81,
        RequestNodeInfoSuccessful = 0x84
    }

}

