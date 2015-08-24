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

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

using SerialPortLib;

using ZWaveLib.Values;
using ZWaveLib.CommandClasses;

namespace ZWaveLib
{
    /// <summary>
    /// Z-Wave Controller.
    /// </summary>
    public class ZWaveController : IDisposable
    {
        
        #region Private fields

        private SerialPortInput serialPort;

        private ManualResetEvent sendMessageAck = new ManualResetEvent(false);
        private ManualResetEvent queueEmptyAck = new ManualResetEvent(false);

        private ZWaveMessage pendingRequest;
        private QueryStage currentStage;
        private List<ZWaveMessage> queuedMessages;
        private Task queueManager;

        private bool discoveryRunning = false;

        private ControllerStatus controllerStatus = ControllerStatus.Disconnected;

        private List<ZWaveNode> nodeList = new List<ZWaveNode>();

        private byte[] lastMessage = null;
        private DateTime lastMessageTimestamp = DateTime.UtcNow;

        private string portName = "";

        #endregion

        #region Public events

        /// <summary>
        /// Controller status changed event handler.
        /// </summary>
        public delegate void ControllerStatusChangedEventHandler(object sender, ControllerStatusEventArgs args);

        /// <summary>
        /// Occurs when controller status changed.
        /// </summary>
        public event ControllerStatusChangedEventHandler ControllerStatusChanged;

        /// <summary>
        /// Discovery progress event handler.
        /// </summary>
        public delegate void DiscoveryProgressEventHandler(object sender, DiscoveryProgressEventArgs args);

        /// <summary>
        /// Occurs during discovery process.
        /// </summary>
        public event DiscoveryProgressEventHandler DiscoveryProgress;

        /// <summary>
        /// Node operation progress event handler.
        /// </summary>
        public delegate void NodeOperationProgressEventHandler(object sender, NodeOperationProgressEventArgs args);

        /// <summary>
        /// Occurs when a node operation is taking place.
        /// </summary>
        public event NodeOperationProgressEventHandler NodeOperationProgress;

        /// <summary>
        /// Node updated event handler.
        /// </summary>
        public delegate void NodeUpdatedEventHandler(object sender, NodeUpdatedEventArgs args);

        /// <summary>
        /// Occurs when node data is updated.
        /// </summary>
        public event NodeUpdatedEventHandler NodeUpdated;

        #endregion

        #region Lifecycle

        /// <summary>
        /// Initializes a new instance of the <see cref="ZWaveLib.ZWaveController"/> class.
        /// </summary>
        public ZWaveController()
        {
            serialPort = new SerialPortInput();
            serialPort.MessageReceived += SerialPort_MessageReceived;
            serialPort.ConnectionStatusChanged += SerialPort_ConnectionStatusChanged;
            queuedMessages = new List<ZWaveMessage>();
            queueManager = new Task(QueueManagerTask);
            queueManager.Start();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZWaveLib.ZWaveController"/> class.
        /// </summary>
        /// <param name="portName">The serial port name.</param>
        public ZWaveController(string portName) : this()
        {
            PortName = portName;
        }

        public void Dispose()
        {
            Disconnect();
            SaveNodesConfig();
        }

        #endregion

        #region Public members

        /// <summary>
        /// Connect this instance.
        /// </summary>
        public void Connect()
        {
            LoadNodesConfig();
            ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
            {
                serialPort.Connect();
            }));
        }

        /// <summary>
        /// Disconnect this instance.
        /// </summary>
        public void Disconnect()
        {
            serialPort.Disconnect();
        }

        /// <summary>
        /// Gets or sets the name of the serial port.
        /// </summary>
        /// <value>The name of the port.</value>
        public string PortName
        {
            get { return portName; }
            set
            {
                portName = value;
                serialPort.SetPort(value);
            }
        }

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>The status.</value>
        public ControllerStatus Status
        {
            get { return controllerStatus; }
        }

        #region Controller commands

        /// <summary>
        /// Queues the message.
        /// </summary>
        /// <returns>The ZWaveMessage object itself.</returns>
        /// <param name="message">Message.</param>
        public ZWaveMessage QueueMessage(ZWaveMessage message)
        {
            queuedMessages.Add(message);
            queueEmptyAck.Reset();
            message.sentAck.Reset();
            return message;
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <returns>True if sending succesfull, False otherwise.</returns>
        /// <param name="message">Message.</param>
        public bool SendMessage(ZWaveMessage message)
        {
            #region Debug 
            Utility.logger.Trace("[[[ BEGIN REQUEST ]]]");
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            #endregion
            SetQueryStage(QueryStage.WaitAck);
            pendingRequest = message;
            sendMessageAck.Reset();
            Utility.logger.Trace("Sending Message (Node={0}, CallbackId={1}, Function={2}, CommandClass={3})", pendingRequest.NodeId, pendingRequest.CallbackId.ToString("X2"), pendingRequest.Function, pendingRequest.CommandClass);
            if (serialPort.SendMessage(message.RawData))
            {
                if (!sendMessageAck.WaitOne(ZWaveMessage.SendMessageTimeoutMs))
                {
                    SetQueryStage(QueryStage.Error);
                    // TODO: Dump Diagnostic Statistics
                    Utility.logger.Warn("Message timeout (Node={0}, CallbackId={0}, Function={1}, CommandClass={2})", pendingRequest.NodeId, pendingRequest.CallbackId.ToString("X2"), pendingRequest.Function, pendingRequest.CommandClass);
                    if (message.NodeId > 1)
                        UpdateOperationProgress(message.NodeId, NodeQueryStatus.Timeout);
                    //System.Diagnostics.Debugger.Break();
                }
            }
            else
            {
                SetQueryStage(QueryStage.Error);
                Utility.logger.Warn("Controller status error (Node={0}, CallbackId={0}, Function={1}, CommandClass={2})", pendingRequest.NodeId, pendingRequest.CallbackId.ToString("X2"), pendingRequest.Function, pendingRequest.CommandClass);
            }
            pendingRequest = null;
            #region Debug 
            stopWatch.Stop();
            Utility.logger.Trace("[[[ END REQUEST ]]] took {0} ms", stopWatch.ElapsedMilliseconds);
            #endregion
            return (currentStage != QueryStage.Error);
        }

        /// <summary>
        /// SoftReset the controller.
        /// </summary>
        public void SoftReset()
        {
            Utility.logger.Trace("BEGIN");
            byte[] message = new byte[] {
                (byte)FrameHeader.SOF,
                0x04,
                (byte)MessageType.Request,
                (byte)ZWaveFunction.ControllerSoftReset,
                0xff,
                0x00
            };
            SendMessage(new ZWaveMessage(message, MessageDirection.Outbound, true));
            Utility.logger.Trace("END");
        }

        /// <summary>
        /// HardReset the controller.
        /// </summary>
        public void HardReset()
        {
            Utility.logger.Trace("BEGIN");
            byte[] message = new byte[] {
                (byte)FrameHeader.SOF,
                0x04,
                (byte)MessageType.Request,
                (byte)ZWaveFunction.ControllerSetDefault,
                0xff,
                0x00
            };
            SendMessage(new ZWaveMessage(message, MessageDirection.Outbound, false));
            nodeList.Clear();
            Utility.logger.Trace("END");
        }

        #endregion

        #region ZWave Discovery / Node Querying

        /// <summary>
        /// Initialize the controller (get the node list).
        /// </summary>
        public void Initialize()
        {
            Utility.logger.Trace("BEGIN");
            Thread.Sleep(1000);
            OnControllerStatusChanged(new ControllerStatusEventArgs(ControllerStatus.Initializing));
            var initialized = SendMessage(new ZWaveMessage(new byte[] { 0x01, 0x03, 0x00, (byte)ZWaveFunction.GetInitData, 0xFE }, MessageDirection.Outbound, false));
            if (initialized)
                OnControllerStatusChanged(new ControllerStatusEventArgs(ControllerStatus.Ready));
            else
                OnControllerStatusChanged(new ControllerStatusEventArgs(ControllerStatus.Error));
            Utility.logger.Trace("END");
        }

        /// <summary>
        /// Query capabilities, Supported Classes and Manufacturer Specific of each node.
        /// </summary>
        public void Discovery()
        {
            // Discovery can be a long time consuming operation, so we ensure
            // that only one instance of this is running
            if (!discoveryRunning)
            {
                discoveryRunning = true;
                OnDiscoveryProgress(new DiscoveryProgressEventArgs(DiscoveryStatus.DiscoveryStart));
                foreach (ZWaveNode zn in nodeList)
                {
                    Utility.logger.Trace("Querying/Updating node {0}", zn.Id);
                    // Get Generic/Basic/Specific Class if not already cached
                    if (zn.ProtocolInfo.BasicType == 0 && zn.ProtocolInfo.GenericType == 0 && zn.ProtocolInfo.SpecificType == 0)
                        GetNodeProtocolInfo(zn.Id);
                    
                    // TODO: should check for SecureNodeInformationFrame as well??

                    // NIF, if cached just return the cached value
                    if (zn.NodeInformationFrame.Length == 0)
                        GetNodeInformationFrame(zn.Id);
                    else
                        OnNodeUpdated(new NodeUpdatedEventArgs(zn.Id, new NodeEvent(zn, EventParameter.NodeInfo, BitConverter.ToString(zn.NodeInformationFrame).Replace("-", " "), 0)));

                    // For nodes that support version command class, query each one for its version.
                    if (zn.SupportCommandClass(CommandClass.Version))
                    {
                        // Compile a list of all of our command class IDs
                        foreach (var cmdClass in zn.CommandClasses)
                        {
                            // if not cached query the node.
                            if (cmdClass.Version == 0)
                                ZWaveLib.CommandClasses.Version.Get(zn, cmdClass.CommandClass).Wait();
                        }
                    }

                    // Manufacturer Specific, if cached just return the cached value
                    if (String.IsNullOrWhiteSpace(zn.ManufacturerSpecific.ManufacturerId))
                        ManufacturerSpecific.Get(zn);
                    else
                        zn.OnNodeUpdated(new NodeEvent(zn, EventParameter.ManufacturerSpecific, zn.ManufacturerSpecific, 0));
                    // Raise the node updated event
                    UpdateOperationProgress(zn.Id, NodeQueryStatus.NodeUpdated);
                }
                SaveNodesConfig();
                OnDiscoveryProgress(new DiscoveryProgressEventArgs(DiscoveryStatus.DiscoveryEnd));
                discoveryRunning = false;
            }
            else
            {
                Utility.logger.Warn("Discovery already running");
            }
        }

        /// <summary>
        /// Gets the node information frame.
        /// </summary>
        /// <returns>The ZWaveMessage object of this request.</returns>
        /// <param name="nodeId">Node identifier.</param>
        public ZWaveMessage GetNodeInformationFrame(byte nodeId)
        {
            Utility.logger.Debug("Node {0}", nodeId);
            byte[] message = new byte[] {
                (byte)FrameHeader.SOF,
                0x04,
                (byte)MessageType.Request,
                (byte)ZWaveFunction.RequestNodeInfo,
                nodeId,
                0x00
            };
            return QueueMessage(new ZWaveMessage(message, MessageDirection.Outbound, false)).Wait();
        }

        /// <summary>
        /// Gets the node protocol info.
        /// </summary>
        /// <returns>The ZWaveMessage object of this request.</returns>
        /// <param name="nodeId">Node identifier.</param>
        public ZWaveMessage GetNodeProtocolInfo(byte nodeId)
        {
            Utility.logger.Debug("Node {0}", nodeId);
            byte[] message = new byte[] {
                (byte)FrameHeader.SOF,
                0x04,
                (byte)MessageType.Request,
                (byte)ZWaveFunction.GetNodeProtocolInfo,
                nodeId,
                0x00
            };
            return QueueMessage(new ZWaveMessage(message, MessageDirection.Outbound, false)).Wait();
        }

        /// <summary>
        /// Gets the node.
        /// </summary>
        /// <returns>The node.</returns>
        /// <param name="nodeId">Node identifier.</param>
        public ZWaveNode GetNode(byte nodeId)
        {
            return nodeList.Find(zn => zn.Id == nodeId);
        }

        /// <summary>
        /// Gets the nodes.
        /// </summary>
        /// <value>The nodes.</value>
        public List<ZWaveNode> Nodes
        {
            get { return nodeList; }
        }

        #endregion

        #region Node Add/Remove

        /// <summary>
        /// Begins the node add.
        /// </summary>
        /// <returns>The ZWaveMessage object of this request.</returns>
        /// <returns>The node add.</returns>
        public ZWaveMessage BeginNodeAdd()
        {
            Utility.logger.Trace("BEGIN");
            byte[] header = new byte[] {
                (byte)FrameHeader.SOF, /* Start Of Frame */
                0x05, /*packet len */
                (byte)MessageType.Request, /* Type of message */
                (byte)ZWaveFunction.NodeAdd
            };
            byte[] footer = new byte[] { (byte)NodeFunctionOption.AddNodeAny | 0x80, 0x00, 0x00 };
            byte[] message = new byte[header.Length + footer.Length];

            System.Array.Copy(header, 0, message, 0, header.Length);
            System.Array.Copy(footer, 0, message, message.Length - footer.Length, footer.Length);

            var msg = QueueMessage(new ZWaveMessage(message, MessageDirection.Outbound, true)).Wait();
            Utility.logger.Trace("END");
            return msg;
        }

        /// <summary>
        /// Stops the node add.
        /// </summary>
        /// <returns>The ZWaveMessage object of this request.</returns>
        /// <returns>The node add.</returns>
        public ZWaveMessage StopNodeAdd()
        {
            Utility.logger.Trace("BEGIN");
            byte[] header = new byte[] {
                (byte)FrameHeader.SOF, /* Start Of Frame */
                0x05 /*packet len */,
                (byte)MessageType.Request, /* Type of message */
                (byte)ZWaveFunction.NodeAdd
            };
            byte[] footer = new byte[] { (byte)NodeFunctionOption.AddNodeStop, 0x00, 0x00 };
            byte[] message = new byte[header.Length + footer.Length];

            System.Array.Copy(header, 0, message, 0, header.Length);
            System.Array.Copy(footer, 0, message, message.Length - footer.Length, footer.Length);

            var msg = QueueMessage(new ZWaveMessage(message, MessageDirection.Outbound, true)).Wait();
            Utility.logger.Trace("END");
            return msg;
        }

        /// <summary>
        /// Begins the node remove.
        /// </summary>
        /// <returns>The ZWaveMessage object of this request.</returns>
        /// <returns>The node remove.</returns>
        public ZWaveMessage BeginNodeRemove()
        {
            Utility.logger.Trace("BEGIN");
            byte[] header = new byte[] {
                (byte)FrameHeader.SOF, /* Start Of Frame */
                0x05 /*packet len */,
                (byte)MessageType.Request, /* Type of message */
                (byte)ZWaveFunction.NodeRemove
            };
            byte[] footer = new byte[] { (byte)NodeFunctionOption.RemoveNodeAny | 0x80, 0x00, 0x00 };
            byte[] message = new byte[header.Length + footer.Length];

            System.Array.Copy(header, 0, message, 0, header.Length);
            System.Array.Copy(footer, 0, message, message.Length - footer.Length, footer.Length);

            var msg = QueueMessage(new ZWaveMessage(message, MessageDirection.Outbound, true)).Wait();
            Utility.logger.Trace("END");
            return msg;
        }

        /// <summary>
        /// Stops the node remove.
        /// </summary>
        /// <returns>The ZWaveMessage object of this request.</returns>
        /// <returns>The node remove.</returns>
        public ZWaveMessage StopNodeRemove()
        {
            Utility.logger.Trace("BEGIN");
            byte[] header = new byte[] {
                (byte)FrameHeader.SOF, /* Start Of Frame */
                0x05 /*packet len */,
                (byte)MessageType.Request, /* Type of message */
                (byte)ZWaveFunction.NodeRemove
            };
            byte[] footer = new byte[] { (byte)NodeFunctionOption.RemoveNodeStop, 0x00, 0x00 };
            byte[] message = new byte[header.Length + footer.Length];

            System.Array.Copy(header, 0, message, 0, header.Length);
            System.Array.Copy(footer, 0, message, message.Length - footer.Length, footer.Length);

            var msg = QueueMessage(new ZWaveMessage(message, MessageDirection.Outbound, true)).Wait();
            Utility.logger.Trace("END");
            return msg;
        }

        #endregion

        #region Node Neighbors Update / Routing Info

        /// <summary>
        /// Requests the neighbors options update.
        /// </summary>
        /// <returns>The ZWaveMessage object of this request.</returns>
        /// <param name="nodeId">Node identifier.</param>
        public ZWaveMessage RequestNeighborsUpdateOptions(byte nodeId)
        {
            Utility.logger.Debug("Node {0}", nodeId);
            var msg = new byte[] {
                (byte)FrameHeader.SOF,
                0x06, /* packet length */
                (byte)MessageType.Request, /* Type of message */
                (byte)ZWaveFunction.RequestNodeNeighborsUpdateOptions,
                nodeId,
                0x25,
                0x00,
                0x00
            };
            return QueueMessage(new ZWaveMessage(msg, MessageDirection.Outbound, true)).Wait();
        }

        /// <summary>
        /// Requests the neighbors update.
        /// </summary>
        /// <returns>The ZWaveMessage object of this request.</returns>
        /// <param name="nodeId">Node identifier.</param>
        public ZWaveMessage RequestNeighborsUpdate(byte nodeId)
        {
            Utility.logger.Debug("Node {0}", nodeId);
            var msg = new byte[] {
                (byte)FrameHeader.SOF,
                0x05, /* packet length */
                (byte)MessageType.Request, /* Type of message */
                (byte)ZWaveFunction.RequestNodeNeighborsUpdate,
                nodeId,
                0x00,
                0x00    
            };
            return QueueMessage(new ZWaveMessage(msg, MessageDirection.Outbound, true)).Wait();
        }

        /// <summary>
        /// Gets the neighbors routing info.
        /// </summary>
        /// <returns>The ZWaveMessage object of this request.</returns>
        /// <param name="nodeId">Node identifier.</param>
        public ZWaveMessage GetNeighborsRoutingInfo(byte nodeId)
        {
            Utility.logger.Debug("Node {0}", nodeId);
            var msg = new byte[] {
                (byte)FrameHeader.SOF,
                0x07, /* packet length */
                (byte)MessageType.Request, /* Type of message */
                (byte)ZWaveFunction.GetRoutingInfo,
                nodeId,
                0x00,
                0x00,
                0x03,
                0x00    
            };
            return QueueMessage(new ZWaveMessage(msg, MessageDirection.Outbound, false)).Wait();
        }

        #endregion

        #endregion

        #region Private members

        #region ZWave Message handling

        private void QueueManagerTask()
        {
            // TODO: implement cancellation token
            while (true)
            {
                while (queuedMessages.Count > 0)
                {
                    var msg = queuedMessages[0];
                    queuedMessages.Remove(msg);

                    if (controllerStatus == ControllerStatus.Ready)
                    {
                        while (!SendMessage(msg) && msg.ResendCount < ZWaveMessage.ResendAttemptsMax)
                        {
                            msg.ResendCount++;
                            Utility.logger.Warn("Could not deliver message to Node {0} (CallbackId={1}, Retry={2})", msg.NodeId, msg.CallbackId.ToString("X2"), msg.ResendCount);
                        }
                        msg.sentAck.Set();
                    
                        if (msg.ResendCount == ZWaveMessage.ResendAttemptsMax)
                        {
                            Utility.logger.Warn("Delivery of message to Node {0} failed (CallbackId={1}).", msg.NodeId, msg.CallbackId.ToString("X2"));
                            if (msg.NodeId > 1)
                                UpdateOperationProgress(msg.NodeId, NodeQueryStatus.Error);
                        }
                    }
                    else
                    {
                        Utility.logger.Warn("Controller satus not read, delivery of message to Node {0} failed (CallbackId={1}).", msg.NodeId, msg.CallbackId.ToString("X2"));
                        if (msg.NodeId > 1)
                            UpdateOperationProgress(msg.NodeId, NodeQueryStatus.Error);
                    }
                }
                if (queuedMessages.Count == 0)
                    queueEmptyAck.Set();
                // TODO: get rid of this Sleep
                Thread.Sleep(500);
            }
        }

        private void ReceiveMessage(ZWaveMessage msg)
        {
            if (DupeMessageCheck(msg))
            {
                Utility.logger.Warn("Repeated message discarded.");
                return;
            }

            var rawData = msg.RawData;
            switch (msg.Type)
            {

            case MessageType.Request:

                if (nodeList.Count == 0)
                    break;

                switch (msg.Function)
                {

                case ZWaveFunction.NotSet:
                    break;

                case ZWaveFunction.NodeAdd:
                    
                    var nodeAddStatus = NodeAddStatus.None;
                    Enum.TryParse(rawData[5].ToString(), out nodeAddStatus);
                    switch (nodeAddStatus)
                    {

                    case NodeAddStatus.LearnReady:
                        
                        UpdateOperationProgress(0x01, NodeQueryStatus.NodeAddReady);
                        SetQueryStage(QueryStage.Complete);
                        break;

                    case NodeAddStatus.AddingSlave:
                        
                        var newNode = AddNode(rawData[6], 0x00);
                        // Extract node information frame
                        int nodeInfoLength = (int)rawData[7];
                        // we don't need to exclude the last 2 CommandClasses
                        byte[] nodeInfo = new byte[nodeInfoLength];
                        Array.Copy(rawData, 8, nodeInfo, 0, nodeInfoLength);
                        newNode.NodeInformationFrame = nodeInfo;
                        newNode.ProtocolInfo.BasicType = rawData[8];
                        newNode.ProtocolInfo.GenericType = rawData[9];
                        newNode.ProtocolInfo.SpecificType = rawData[10];
                        nodeList.Add(newNode);

                        UpdateOperationProgress(newNode.Id, NodeQueryStatus.NodeAddStarted);

                        if (newNode.SupportCommandClass(CommandClass.Security))
                        {
                            var nodeSecurityData = Security.GetSecurityData(newNode);
                            nodeSecurityData.IsAddingNode = true;

                            Security.GetScheme(newNode);
                        }
                        else
                        {
                            NodeInformationFrameDone(newNode);
                        }
                        break;

                    case NodeAddStatus.ProtocolDone:

                        GetNodeProtocolInfo(rawData[6]);
                        var addedNode = GetNode(rawData[6]);
                        if (addedNode != null)
                        {
                            ManufacturerSpecific.Get(addedNode);
                            UpdateOperationProgress(addedNode.Id, NodeQueryStatus.NodeAddDone);
                        }
                        else
                        {
                            UpdateOperationProgress(rawData[6], NodeQueryStatus.NodeAddFailed);
                        }
                        SetQueryStage(QueryStage.Complete);
                        break;

                    case NodeAddStatus.Done:
                        
                        UpdateOperationProgress(0x01, NodeQueryStatus.NodeAddDone);
                        SetQueryStage(QueryStage.Complete);
                        break;

                    case NodeAddStatus.Failed:

                        UpdateOperationProgress(rawData[6], NodeQueryStatus.NodeAddFailed);
                        SetQueryStage(QueryStage.Complete);
                        break;

                    }
                    break;

                case ZWaveFunction.NodeRemove:

                    var nodeRemoveStatus = NodeRemoveStatus.None;
                    Enum.TryParse(rawData[5].ToString(), out nodeRemoveStatus);
                    switch (nodeRemoveStatus)
                    {

                    case NodeRemoveStatus.LearnReady:
                        
                        UpdateOperationProgress(0x01, NodeQueryStatus.NodeRemoveReady);
                        SetQueryStage(QueryStage.Complete);
                        break;

                    case NodeRemoveStatus.RemovingSlave:

                        UpdateOperationProgress(rawData[6], NodeQueryStatus.NodeRemoveStarted);
                        break;
                    
                    case NodeRemoveStatus.Done:

                        if (rawData[6] != 0x00)
                            RemoveNode(rawData[6]);
                        UpdateOperationProgress(rawData[6], NodeQueryStatus.NodeRemoveDone);
                        SetQueryStage(QueryStage.Complete);
                        break;

                    case NodeRemoveStatus.Failed:

                        UpdateOperationProgress(rawData[6], NodeQueryStatus.NodeRemoveFailed);
                        SetQueryStage(QueryStage.Complete);
                        break;

                    }
                    break;

                case ZWaveFunction.RequestNodeNeighborsUpdateOptions:
                case ZWaveFunction.RequestNodeNeighborsUpdate:

                    var neighborUpdateStatus = NeighborsUpdateStatus.None;
                    Enum.TryParse(rawData[5].ToString(), out neighborUpdateStatus);
                    switch (neighborUpdateStatus)
                    {

                    case NeighborsUpdateStatus.NeighborsUpdateStared:

                        UpdateOperationProgress(msg.NodeId, NodeQueryStatus.NeighborUpdateStarted);
                        break;

                    case NeighborsUpdateStatus.NeighborsUpdateDone:

                        UpdateOperationProgress(msg.NodeId, NodeQueryStatus.NeighborUpdateDone);
                        //GetNeighborsRoutingInfo(msg.NodeId);
                        SetQueryStage(QueryStage.Complete);
                        break;

                    case NeighborsUpdateStatus.NeighborsUpdateFailed:

                        UpdateOperationProgress(msg.NodeId, NodeQueryStatus.NeighborUpdateFailed);
                        SetQueryStage(QueryStage.Complete);
                        break;

                    default:
                        Utility.logger.Warn("Unhandled Node Neighbor Update request: {0}", BitConverter.ToString(rawData));
                        break;

                    }
                    break;

                case ZWaveFunction.SendData:

                    byte callbackId = rawData[4];
                    if (callbackId == 0x01) // 0x01 is "SEND DATA OK"
                    {
                        // TODO: ... is there anything to be done here?
                    }
                    else
                    {
                        switch (msg.CallbackStatus)
                        {

                        case CallbackStatus.Ack:
                            //System.Diagnostics.Debugger.Break();
                            break;

                        case CallbackStatus.Nack:
                            //System.Diagnostics.Debugger.Break();
                            break;

                        }
                    }
                    break;

                case ZWaveFunction.ApplicationCommandHandler:

                    var node = GetNode(rawData[5]);
                    if (node != null)
                    {
                        try
                        {
                            node.ApplicationCommandHandler(rawData);
                        }
                        catch (Exception ex)
                        {
                            Utility.logger.Error(ex);
                        }
                    }
                    else
                    {
                        Utility.logger.Error("Unknown node id {0}", rawData[5]);
                    }
                    break;

                case ZWaveFunction.ApplicationUpdate:

                    int nifLength = (int)rawData[6];
                    var znode = GetNode(rawData[5]);
                    if (znode != null)
                    {
                        // we don't need to exclude the last 2 CommandClasses
                        byte[] nodeInfo = new byte[nifLength];
                        Array.Copy(rawData, 7, nodeInfo, 0, nifLength);
                        znode.NodeInformationFrame = nodeInfo;
                        if (znode.SupportCommandClass(CommandClass.Security))
                        {
                            // ask the node what security command classes are supported
                            Security.GetSupported(znode);
                        }
                        else
                        {
                            NodeInformationFrameDone(znode);
                            SetQueryStage(QueryStage.Complete);
                        }
                    }
                    else
                    {
                        SetQueryStage(QueryStage.Error);
                    }
                    break;

                default:
                    Utility.logger.Warn("Unhandled request message: {0}", BitConverter.ToString(rawData));
                    break;

                }

                break;

            case MessageType.Response:

                switch (msg.Function)
                {

                case ZWaveFunction.GetInitData:
                    InitializeNodes(rawData);
                    SetQueryStage(QueryStage.Complete);
                    break;

                case ZWaveFunction.GetNodeProtocolInfo:
                    var node = GetNode(msg.NodeId);
                    node.ProtocolInfo.BasicType = rawData[7];
                    node.ProtocolInfo.GenericType = rawData[8];
                    node.ProtocolInfo.SpecificType = rawData[9];
                    break;

                case ZWaveFunction.RequestNodeInfo:
                    SetQueryStage(QueryStage.SendDataReady);
                    break;

                case ZWaveFunction.SendData:
                    // TODO: shall we do something here?
                    break;

                case ZWaveFunction.GetRoutingInfo:
                    var routingInfo = Utility.ExtractRoutingFromBitMask(rawData);
                    if (routingInfo.Length > 0)
                    {
                        var routedNode = GetNode(msg.NodeId);
                        if (routedNode != null)
                        {
                            routedNode.UpdateData("RoutingInfo", routingInfo);
                            routedNode.OnNodeUpdated(new NodeEvent(routedNode, EventParameter.RoutingInfo, String.Join(" ", routingInfo), 0));
                        }
                    }
                    else
                    {
                        Utility.logger.Warn("No routing nodes reported.");
                    }
                    break;

                default:
                    Utility.logger.Warn("Unhandled response message: {0}", BitConverter.ToString(rawData));
                    break;

                }

                break;

            default:
                Utility.logger.Warn("Unhandled message type: {0}", BitConverter.ToString(rawData));
                break;
            }

        }

        /// <summary>
        /// Sets the query stage.
        /// </summary>
        /// <param name="stage">Stage.</param>
        private void SetQueryStage(QueryStage stage)
        {
            Utility.logger.Trace(stage);
            currentStage = stage;
            // If query stage is complete, unlock SendMessage
            if (stage == QueryStage.Complete || stage == QueryStage.Error)
                sendMessageAck.Set();
        }

        /// <summary>
        /// Updates the query stage.
        /// </summary>
        /// <param name="zm">Zm.</param>
        private void UpdateQueryStage(ZWaveMessage zm)
        {
            if (currentStage != QueryStage.Complete && currentStage != QueryStage.NotSet && currentStage != QueryStage.Error)
            {
                //Utility.logger.Trace("Query Stage {0} Type {1} Function {2}={3} Node {4}={5} Callback {6}={7}", currentStage, zm.Type, zm.Function, currentMessage.Function, zm.NodeId, currentMessage.NodeId, zm.CallbackId, currentMessage.CallbackId);

                switch (currentStage)
                {
                case QueryStage.WaitAck:
                    // The controller accepted a request
                    if (zm.Type == MessageType.Response && zm.Function == pendingRequest.Function)
                    {
                        if (pendingRequest.CallbackId == 0)
                        {
                            SetQueryStage(QueryStage.Complete);
                        }
                        else
                        {
                            // The controller needs querying data from the node
                            SetQueryStage(QueryStage.SendDataReady);
                        }
                    }
                    break;
                case QueryStage.SendDataReady:
                    // The controller requested data from the node
                    if (zm.Type == MessageType.Request && zm.Function == pendingRequest.Function)
                    {
                        if (zm.CallbackStatus != CallbackStatus.Ack)
                        {
                            SetQueryStage(QueryStage.Error);
                            // TODO: Dump Diagnostic Statistics
                        }
                        else
                        {
                            SetQueryStage(QueryStage.WaitData);
                            zm.NodeId = pendingRequest.NodeId;
                        }
                    }
                    break;
                case QueryStage.WaitData:
                    // got the data from the node
                    zm.NodeId = pendingRequest.NodeId;
                    zm.CallbackId = pendingRequest.CallbackId;
                    SetQueryStage(QueryStage.Complete);
                    break;
                }
            }
        }

        /// <summary>
        /// Processes a ZWave message.
        /// </summary>
        /// <param name="zm">Zm.</param>
        private void ProcessMessage(ZWaveMessage zm)
        {
            if (zm.Header == FrameHeader.SOF)
            {

                if (ZWaveMessage.VerifyChecksum(zm.RawData))
                {

                    // Some replies do not include the Id of the node
                    // so we take it from the pending request message
                    if (pendingRequest != null && zm.NodeId == 0)
                    {
                        zm.NodeId = pendingRequest.NodeId;
                        zm.CallbackId = pendingRequest.CallbackId;
                    }

                    SendAck();
                    ReceiveMessage(zm);
                    UpdateQueryStage(zm);

                }
                else
                {
                    SendNack();
                    Utility.logger.Warn("Bad message checksum");
                }

            }
            else if (zm.Header == FrameHeader.CAN)
            {
                SetQueryStage(QueryStage.Error);
            }
            else
            {
                Utility.logger.Warn("Unhandled message type: {0}", BitConverter.ToString(zm.RawData));
            }
        }

        private void SendAck()
        {
            serialPort.SendMessage(ZWaveMessage.Ack);
        }

        private void SendNack()
        {
            serialPort.SendMessage(ZWaveMessage.Nack);
        }

        private bool DupeMessageCheck(ZWaveMessage msg)
        {
            // Discard repeated messages within last 2 seconds time range
            bool repeated = false;
            if (lastMessage != null)
            {
                var elapsed = (DateTime.UtcNow - lastMessageTimestamp);
                if (elapsed.TotalSeconds <= 2 && lastMessage.SequenceEqual(msg.RawData))
                {
                    repeated = true;
                }
            }
            lastMessageTimestamp = DateTime.UtcNow;
            lastMessage = new byte[msg.RawData.Length];
            Buffer.BlockCopy(msg.RawData, 0, lastMessage, 0, msg.RawData.Length * sizeof(byte));
            return repeated;
        }

        #endregion

        #region ZWaveNode event handlers

        private void ZWave_NodeUpdated(object sender, NodeEvent eventData)
        {
            ZWaveNode node = (ZWaveNode)sender;
            if (eventData.Parameter == EventParameter.SecurityDecriptedMessage && eventData.Value is byte[])
            {
                node.ApplicationCommandHandler((byte[])eventData.Value);
                return;
            }
            else if (eventData.Parameter == EventParameter.SecurityNodeInformationFrame)
            {
                node.SecuredNodeInformationFrame = (byte[])eventData.Value;

                // we take them one a a time to make sure we keep the list with unique elements
                foreach (byte nodeInfo in node.SecuredNodeInformationFrame)
                {
                    // if we found the COMMAND_CLASS_MARK we get out of the for loop
                    if (nodeInfo == (byte)0xEF)
                        break;
                    node.NodeInformationFrame = Utility.AppendByteToArray(node.NodeInformationFrame, nodeInfo);
                }
                // we just send other events and save the node data
                NodeInformationFrameDone(node);
            }
            // Route node event
            OnNodeUpdated(new NodeUpdatedEventArgs(eventData.Node.Id, eventData));
        }

        #endregion

        #region Serial Port events and data parsing

        /// <summary>
        /// Parses the data buffer coming from the serial port.
        /// </summary>
        /// <param name="message">raw bytes data.</param>
        private void ParseSerialData(byte[] message)
        {
            // Extract Z-Wave frames from incoming serial port data
            FrameHeader header = (FrameHeader)((int)message[0]);
            if (header == FrameHeader.ACK)
            {
                if (message.Length > 1)
                {
                    byte[] msg = new byte[message.Length - 1];
                    Array.Copy(message, 1, msg, 0, msg.Length);
                    ProcessMessage(new ZWaveMessage(msg, MessageDirection.Inbound));
                }
                return;
            }

            int msgLength = 0;
            byte[] nextMessage = null;
            if (message.Length > 1)
            {
                msgLength = (int)message[1];
                if (message.Length > msgLength + 2)
                {
                    nextMessage = new byte[message.Length - msgLength - 2];
                    Array.Copy(message, msgLength + 2, nextMessage, 0, nextMessage.Length);
                    byte[] tmpmsg = new byte[msgLength + 2];
                    Array.Copy(message, 0, tmpmsg, 0, msgLength + 2);
                    message = tmpmsg;
                }
            }

            try
            {
                ProcessMessage(new ZWaveMessage(message, MessageDirection.Inbound));
            }
            catch (Exception e)
            {
                Utility.logger.Error(e);
            }

            if (nextMessage != null)
            {
                // TODO: Check out possible recursion loops
                ParseSerialData(nextMessage);
            }
        }

        private void SerialPort_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
        {
            var status = args.Connected ? ControllerStatus.Connected : ControllerStatus.Disconnected;
            Thread.Sleep(1000);
            OnControllerStatusChanged(new ControllerStatusEventArgs(status));
        }

        private void SerialPort_MessageReceived(object sender, SerialPortLib.MessageReceivedEventArgs args)
        {
            ParseSerialData(args.Data);
        }

        #endregion

        #region Node management and configuration persistence

        private void InitializeNodes(byte[] receivedMessage)
        {
            var nodes = Utility.ExtractNodesFromBitMask(receivedMessage);
            foreach (byte i in nodes)
            {
                // i = 0x01 is the controller itself, so we don't add it to the nodelist
                if (i == 0x01)
                    continue;
                if (GetNode(i) == null)
                    nodeList.Add(AddNode(i, 0x00));
            }
        }

        private ZWaveNode AddNode(byte nodeId, byte genericClass)
        {
            ZWaveNode node;
            switch (genericClass)
            {
            case (byte) GenericType.StaticController:
                // TODO: what should be done here?...
                node = null;
                break;
            default: // generic node
                node = new ZWaveNode(this, nodeId, genericClass);
                break;
            }
            node.NodeUpdated += ZWave_NodeUpdated;
            UpdateOperationProgress(nodeId, NodeQueryStatus.NodeAdded);
            return node;
        }

        private void RemoveNode(byte nodeId)
        {
            var node = GetNode(nodeId);
            if (node != null)
            {
                node.NodeUpdated -= ZWave_NodeUpdated;
            }
            nodeList.RemoveAll(zn => zn.Id == nodeId);
            UpdateOperationProgress(nodeId, NodeQueryStatus.NodeRemoved);
        }

        private void LoadNodesConfig()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "zwavenodes.xml");
            if (File.Exists(configPath))
            {
                try
                {
                    var serializer = new XmlSerializer(nodeList.GetType());
                    var reader = new StreamReader(configPath);
                    nodeList = (List<ZWaveNode>)serializer.Deserialize(reader);
                    reader.Close();
                    foreach (var node in nodeList)
                    {
                        node.NodeUpdated += ZWave_NodeUpdated;
                        node.SetController(this);
                    }
                }
                catch (Exception e)
                {
                    Utility.logger.Error(e);
                }
            }
        }

        private void SaveNodesConfig()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "zwavenodes.xml");
            try
            {
                var settings = new System.Xml.XmlWriterSettings();
                settings.Indent = true;
                var serializer = new System.Xml.Serialization.XmlSerializer(nodeList.GetType());
                var writer = System.Xml.XmlWriter.Create(configPath, settings);
                serializer.Serialize(writer, nodeList);
                writer.Close();
            }
            catch (Exception e)
            {
                Utility.logger.Error(e);
            }
        }

        private void UpdateOperationProgress(byte nodeId, NodeQueryStatus status)
        {
            OnNodeOperationProgress(new NodeOperationProgressEventArgs(nodeId, status));
        }

        private void NodeInformationFrameDone(ZWaveNode znode)
        {
            znode.UpdateCommandClassList();
            // once we get the security command classes we'll issue the same events and call SaveNodesConfig();
            OnNodeUpdated(new NodeUpdatedEventArgs(znode.Id, new NodeEvent(znode, EventParameter.NodeInfo, BitConverter.ToString(znode.NodeInformationFrame).Replace("-", " "), 0)));
            // TODO: deprecate the WakeUpNotify event?
            OnNodeUpdated(new NodeUpdatedEventArgs(znode.Id, new NodeEvent(znode, EventParameter.WakeUpNotify, "1", 0)));
        }

        #endregion

        #region Controller events

        /// <summary>
        /// Raises the node updated event.
        /// </summary>
        /// <param name="args">Arguments.</param>
        protected virtual void OnNodeUpdated(NodeUpdatedEventArgs args)
        {
            Utility.logger.Debug("{0} {1} {2}", args.NodeId, args.Event.Parameter, args.Event.Value);
            if (NodeUpdated != null)
                new Thread(() =>
                {
                    NodeUpdated(this, args);
                }).Start();
        }

        /// <summary>
        /// Raises the discovery progress event.
        /// </summary>
        /// <param name="args">Arguments.</param>
        protected virtual void OnDiscoveryProgress(DiscoveryProgressEventArgs args)
        {
            Utility.logger.Debug(args.Status);
            if (DiscoveryProgress != null)
                DiscoveryProgress(this, args);
        }

        /// <summary>
        /// Raises the node operation progress event.
        /// </summary>
        /// <param name="args">Arguments.</param>
        protected virtual void OnNodeOperationProgress(NodeOperationProgressEventArgs args)
        {
            Utility.logger.Debug("{0} {1}", args.NodeId, args.Status);
            if (NodeOperationProgress != null)
                NodeOperationProgress(this, args);
        }

        /// <summary>
        /// Raises the controller status changed event.
        /// </summary>
        /// <param name="args">Arguments.</param>
        protected virtual void OnControllerStatusChanged(ControllerStatusEventArgs args)
        {
            controllerStatus = args.Status;
            Utility.logger.Debug("{0}", controllerStatus);
            if (controllerStatus == ControllerStatus.Disconnected)
                queuedMessages.Clear();
            if (ControllerStatusChanged != null)
                ControllerStatusChanged(this, args);
        }

        #endregion

        #endregion

    }

}
