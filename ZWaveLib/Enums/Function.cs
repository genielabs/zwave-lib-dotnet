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

