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
    public class ControllerStatusEventArgs
    {
        public readonly ControllerStatus Status;
        public ControllerStatusEventArgs(ControllerStatus status)
        {
            Status = status;
        }
    }

    public class NodeEvent
    {
        public readonly ZWaveNode Node;
        public readonly EventParameter Parameter;
        public readonly object Value;
        public int Instance { get; internal set; }
        public NodeEvent NestedEvent { get; internal set; }

        public NodeEvent(ZWaveNode node, EventParameter eventType, object eventValue, int instance)
        {
            this.Node = node;
            this.Parameter = eventType;
            this.Value = eventValue;
            this.Instance = instance;
        }
    }

    public class NodeOperationProgressEventArgs
    {
        public readonly byte NodeId;
        public readonly NodeQueryStatus Status;

        public NodeOperationProgressEventArgs(byte nodeId, NodeQueryStatus status)
        {
            this.NodeId = nodeId;
            this.Status = status;
        }
    }

    public class NodeUpdatedEventArgs
    {
        public readonly byte NodeId;
        public readonly NodeEvent Event;

        public NodeUpdatedEventArgs(byte nodeId, NodeEvent evt)
        {
            this.NodeId = nodeId;
            this.Event = evt;
        }
    }

    public class DiscoveryProgressEventArgs
    {
        public readonly DiscoveryStatus Status;

        public DiscoveryProgressEventArgs(DiscoveryStatus status)
        {
            this.Status = status;
        }
    }

    public class HealProgressEventArgs
    {
        public readonly HealStatus Status;

        public HealProgressEventArgs(HealStatus status)
        {
            this.Status = status;
        }
    }

    public class MessageReceivedEventArgs
    {
        public readonly ZWaveMessage Message;

        public MessageReceivedEventArgs(ZWaveMessage message)
        {
            Message = message;
        }
    }

}

