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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using ZWaveLib.CommandClasses;
using System.Xml.Serialization;

namespace ZWaveLib
{
    [Serializable]
    public class NodeCapabilities
    {

        /// <summary>
        /// Gets or sets the basic type.
        /// </summary>
        /// <value>The basic type.</value>
        public byte BasicType { get; internal set; }

        /// <summary>
        /// Gets or sets the generic type.
        /// </summary>
        /// <value>The generic type.</value>
        public byte GenericType { get; internal set; }

        /// <summary>
        /// Gets or sets the specific type.
        /// </summary>
        /// <value>The specific type.</value>
        public byte SpecificType { get; internal set; }

        public NodeCapabilities()
        {
        }

    }

    [Serializable]
    public class NodeCommandClass
    {
        public readonly byte Id;
        public int Version { get; internal set; }
        [XmlIgnore]
        public CommandClass CommandClass { get { return (CommandClass)Id; } }

        public NodeCommandClass()
        {
        }

        public NodeCommandClass(byte id, int version = 0)
        {
            Id = id;
            Version = version;
        }
    }

    public class NodeData
    {
        public string Name { get; internal set; }

        public object Value { get; internal set; }

        public NodeData(string fieldName, object data)
        {
            Name = fieldName;
            Value = data;
        }
    }

    [Serializable]
    public class ZWaveNode
    {
        #region Private fields

        private ZWaveController controller;

        #endregion

        #region Public fields and events

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public byte Id { get; protected set; }

        public NodeCapabilities ProtocolInfo { get; internal set; }

        /// <summary>
        /// Gets or sets the node information frame.
        /// </summary>
        /// <value>The node information frame.</value>
        public byte[] NodeInformationFrame { get; internal set; }

        /// <summary>
        /// Gets or sets the secured node information frame.
        /// </summary>
        /// <value>The secured node information frame.</value>
        public byte[] SecuredNodeInformationFrame { get; internal set; }

        public List<NodeCommandClass> CommandClasses { get; internal set; }

        /// <summary>
        /// Gets or sets the manufacturer specific.
        /// </summary>
        /// <value>The manufacturer specific.</value>
        public ManufacturerSpecificInfo ManufacturerSpecific { get; internal set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        [XmlIgnore]
        public List<NodeData> Data { get; internal set; }

        /// <summary>
        /// Node updated event handler.
        /// </summary>
        public delegate void NodeUpdatedEventHandler(object sender, NodeEvent eventData);

        /// <summary>
        /// Occurs when node is updated.
        /// </summary>
        public event NodeUpdatedEventHandler NodeUpdated;

        #endregion

        #region Lifecycle

        public ZWaveNode()
        {
            Data = new List<NodeData>();
            CommandClasses = new List<NodeCommandClass>();
            ProtocolInfo = new NodeCapabilities();
            NodeInformationFrame = new byte[]{ };
            SecuredNodeInformationFrame = new byte[]{ };
            ManufacturerSpecific = new ManufacturerSpecificInfo();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZWaveLib.ZWaveNode"/> class.
        /// </summary>
        /// <param name="controller">Controller.</param>
        /// <param name="nodeId">Node identifier.</param>
        public ZWaveNode(ZWaveController controller, byte nodeId) : this()
        {
            this.controller = controller;
            Id = nodeId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZWaveLib.ZWaveNode"/> class.
        /// </summary>
        /// <param name="controller">Controller.</param>
        /// <param name="nodeId">Node identifier.</param>
        /// <param name="genericType">Generic type.</param>
        public ZWaveNode(ZWaveController controller, byte nodeId, byte genericType) : this(controller, nodeId)
        {
            ProtocolInfo.GenericType = genericType;
        }

        #endregion

        #region Public members

        /// <summary>
        /// Gets the command class.
        /// </summary>
        /// <returns>The command class.</returns>
        /// <param name="cclass">Cclass.</param>
        public NodeCommandClass GetCommandClass(CommandClass cclass)
        {
            return this.CommandClasses.Find(cc => cc.Id.Equals((byte)cclass));
        }

        /// <summary>
        /// Supports the command class.
        /// </summary>
        /// <returns><c>true</c>, if command class is supported, <c>false</c> otherwise.</returns>
        /// <param name="commandClass">Command Class</param>
        public bool SupportCommandClass(CommandClass commandClass)
        {
            bool isSupported = false;
            isSupported = (Array.IndexOf(NodeInformationFrame, (byte)commandClass) >= 0);
            return isSupported;
        }

        /// <summary>
        /// Determines whether this instance command class specified by c is secured.
        /// </summary>
        /// <returns><c>true</c> true if is secured command class; otherwise, <c>false</c>.</returns>
        /// <param name="commandClass">Command Class.</param>
        public bool IsSecuredCommandClass(CommandClass commandClass)
        {
            bool isSecured = false;
            if (SecuredNodeInformationFrame != null)
            {
                isSecured = (Array.IndexOf(SecuredNodeInformationFrame, (byte)commandClass) >= 0);
            }
            return isSecured;
        }

        /// <summary>
        /// Gets the custom node data.
        /// </summary>
        /// <returns>The data.</returns>
        /// <param name="fieldId">Field identifier.</param>
        /// <param name="defaultValue">Default value.</param>
        public NodeData GetData(string fieldId, object defaultValue = null)
        {
            var item = Data.Find(d => d.Name == fieldId);
            if (item == null)
            {
                if (defaultValue != null)
                {
                    item = new NodeData(fieldId, defaultValue);
                    Data.Add(item);
                }
            }
            return item;
        }

        /// <summary>
        /// Updates the custom node data.
        /// </summary>
        /// <param name="fieldId">Field identifier.</param>
        /// <param name="value">Value.</param>
        public void UpdateData(string fieldId, object value)
        {
            var item = GetData(fieldId, value);
            item.Value = value;
        }

        /// <summary>
        /// Sends the data request.
        /// </summary>
        /// <param name="request">Request.</param>
        public ZWaveMessage SendDataRequest(byte[] request)
        {
            byte cmdClass = request[0];
            byte[] message = ZWaveMessage.BuildSendDataRequest(Id, request);
            // when cmdClass belongs to SecuredNodeInformationFrame we need to encrypt the message
            if (cmdClass != (byte)CommandClass.Security && IsSecuredCommandClass((CommandClass)cmdClass))
            {
                Security.SendMessage(this, message);
                // TODO: not yet supported for Security Command Classs,
                // TODO: update Security.cs class
                return null;
            }
            else
            {
                return SendMessage(message);
            }
        }

        #endregion

        #region Private members

        internal virtual bool ApplicationCommandHandler(byte[] receivedMessage)
        {
            NodeEvent messageEvent = null;
            int messageLength = receivedMessage.Length;

            if (messageLength > 8)
            {
                byte commandLength = receivedMessage[6];
                byte commandClass = receivedMessage[7];
                // TODO: this should be moved inside the NodeCommandClass class
                // TODO: as "Instance" property
                var cc = CommandClassFactory.GetCommandClass(commandClass);
                byte[] message = new byte[commandLength];
                Array.Copy(receivedMessage, 7, message, 0, commandLength);
                try
                {
                    messageEvent = cc.GetEvent(this, message);
                }
                catch (Exception ex)
                {
                    Utility.logger.Error(ex);
                }
            }

            if (messageEvent != null)
            {
                OnNodeUpdated(messageEvent);
            }
            else if (messageLength > 3 && receivedMessage[3] != 0x13)
            {
                Utility.logger.Warn("Unhandled message type");
            }

            return false;
        }

        internal ZWaveMessage SendMessage(byte[] message)
        {
            var msg = new ZWaveMessage(message, MessageDirection.Outbound, true);
            controller.QueueMessage(msg);
            return msg;
        }

        internal void UpdateCommandClassList()
        {
            // we only build the list once
            if (this.CommandClasses.Count != NodeInformationFrame.Length)
            {
                this.CommandClasses.Clear();
                foreach (var cc in NodeInformationFrame)
                {
                    var cclass = CommandClass.NotSet;
                    Enum.TryParse<CommandClass>(cc.ToString(), out cclass);
                    this.CommandClasses.Add(new NodeCommandClass((byte)cclass));
                }
            }
        }

        internal void SetController(ZWaveController controller)
        {
            this.controller = controller;
        }

        internal virtual void OnNodeUpdated(NodeEvent zevent)
        {
            if (NodeUpdated != null)
                NodeUpdated(this, zevent);
        }

        #endregion

    }
}
