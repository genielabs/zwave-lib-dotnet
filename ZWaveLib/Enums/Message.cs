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

using System;

namespace ZWaveLib
{

    public enum QueryStage
    {
        NotSet,
        WaitAck,
        SendDataReady,
        Complete,
        Error
    }

    public enum FrameHeader : byte
    {
        SOF = 0x01,
        ACK = 0x06,
        NAK = 0x15,
        CAN = 0x18
    }

    public enum MessageDirection : byte
    {
        Inbound = 0x00,
        Outbound = 0x01
    }

    public enum MessageType : byte
    {
        Request = 0x00,
        Response = 0x01,
        NotSet = 0xFF
    }

}

