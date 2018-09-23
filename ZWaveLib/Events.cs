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

using System;

namespace ZWaveLib
{
    public class ControllerStatusEventArgs
    {
        public readonly ControllerStatus Status;
        public readonly DateTime Timestamp;

        public ControllerStatusEventArgs(ControllerStatus status)
        {
            Status = status;
            Timestamp = DateTime.UtcNow;
        }
    }

    public class NodeEvent
    {
        public readonly ZWaveNode Node;
        public readonly EventParameter Parameter;
        public readonly object Value;
        public readonly DateTime Timestamp;
        public int Instance { get; internal set; }
        public NodeEvent NestedEvent { get; internal set; }

        public NodeEvent(ZWaveNode node, EventParameter eventType, object eventValue, int instance)
        {
            Node = node;
            Parameter = eventType;
            Value = eventValue;
            Instance = instance;
            Timestamp = DateTime.UtcNow;
        }
    }

    public class NodeOperationProgressEventArgs
    {
        public readonly byte NodeId;
        public readonly NodeQueryStatus Status;
        public readonly DateTime Timestamp;

        public NodeOperationProgressEventArgs(byte nodeId, NodeQueryStatus status)
        {
            NodeId = nodeId;
            Status = status;
            Timestamp = DateTime.UtcNow;
        }
    }

    public class NodeUpdatedEventArgs
    {
        public readonly byte NodeId;
        public readonly NodeEvent Event;
        public readonly DateTime Timestamp;

        public NodeUpdatedEventArgs(byte nodeId, NodeEvent evt)
        {
            NodeId = nodeId;
            Event = evt;
            Timestamp = DateTime.UtcNow;
        }
    }

    public class DiscoveryProgressEventArgs
    {
        public readonly DiscoveryStatus Status;
        public readonly DateTime Timestamp;

        public DiscoveryProgressEventArgs(DiscoveryStatus status)
        {
            Status = status;
            Timestamp = DateTime.UtcNow;
        }
    }

    public class HealProgressEventArgs
    {
        public readonly HealStatus Status;
        public readonly DateTime Timestamp;

        public HealProgressEventArgs(HealStatus status)
        {
            Status = status;
            Timestamp = DateTime.UtcNow;
        }
    }

    public class MessageReceivedEventArgs
    {
        public readonly ZWaveMessage Message;
        public readonly DateTime Timestamp;

        public MessageReceivedEventArgs(ZWaveMessage message)
        {
            Message = message;
            Timestamp = DateTime.UtcNow;
        }
    }

}

