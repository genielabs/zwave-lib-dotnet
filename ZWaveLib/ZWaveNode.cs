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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using ZWaveLib.CommandClasses;
using System.Xml.Serialization;

namespace ZWaveLib
{

    /// <summary>
    /// Node capabilities (Protocol Info).
    /// </summary>
    [Serializable]
    public class NodeCapabilities
    {
        /// <summary>
        /// Gets or sets the basic type.
        /// </summary>
        /// <value>The basic type.</value>
        public byte BasicType { get; /*internal*/ set; }

        /// <summary>
        /// Gets or sets the generic type.
        /// </summary>
        /// <value>The generic type.</value>
        public byte GenericType { get; /*internal*/ set; }

        /// <summary>
        /// Gets or sets the specific type.
        /// </summary>
        /// <value>The specific type.</value>
        public byte SpecificType { get; /*internal*/ set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZWaveLib.NodeCapabilities"/> class.
        /// </summary>
        public NodeCapabilities()
        {
        }
    }

    /// <summary>
    /// Node software version.
    /// </summary>
    [Serializable]
    public class NodeVersion
    {
        /// <summary>
        /// Gets or sets the Z-Wave Library Type.
        /// </summary>
        /// <value>Z-Wave Library Type.</value>
        public byte LibraryType { get; /*internal*/ set; }

        /// <summary>
        /// Gets or sets the Z-Wave Protocol Version.
        /// </summary>
        /// <value>Z-Wave Protocol Version.</value>
        public byte ProtocolVersion { get; /*internal*/ set; }

        /// <summary>
        /// Gets or sets the Z-Wave Protocol Sub Version.
        /// </summary>
        /// <value>Z-Wave Protocol Sub Version.</value>
        public byte ProtocolSubVersion { get; /*internal*/ set; }

        /// <summary>
        /// Gets or sets the Application Version.
        /// </summary>
        /// <value>Application Version.</value>
        public byte ApplicationVersion { get; /*internal*/ set; }

        /// <summary>
        /// Gets or sets the Application Sub Version.
        /// </summary>
        /// <value>Application Sub Version.</value>
        public byte ApplicationSubVersion { get; /*internal*/ set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZWaveLib.NodeVersion"/> class.
        /// </summary>
        public NodeVersion()
        {
        }

        public override string ToString()
        {
            return string.Format("{{\"LibraryType\":{0}, \"ProtocolVersion\":{1}, \"ProtocolSubVersion\":{2}, \"ApplicationVersion\":{3}, \"ApplicationSubVersion\":{4}}}", 
                LibraryType, ProtocolVersion, ProtocolSubVersion, ApplicationVersion, ApplicationSubVersion);
        }
    }

    /// <summary>
    /// Node command class.
    /// </summary>
    [Serializable]
    public class NodeCommandClass
    {
        /// <summary>
        /// The CC identifier.
        /// </summary>
        public /* readonly */ byte Id;

        /// <summary>
        /// Gets or sets the version for this CC.
        /// </summary>
        /// <value>The version.</value>
        public int Version { get; /*internal*/ set; }

        /// <summary>
        /// Gets the command class enumeration entry.
        /// </summary>
        /// <value>The command class.</value>
        [XmlIgnore]
        public CommandClass CommandClass { get { return (CommandClass)Id; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZWaveLib.NodeCommandClass"/> class.
        /// </summary>
        public NodeCommandClass()
        {
            Version = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZWaveLib.NodeCommandClass"/> class.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="version">Version.</param>
        public NodeCommandClass(byte id, int version = -1)
        {
            Id = id;
            Version = version;
        }
    }

    /// <summary>
    /// Custom node data.
    /// </summary>
    public class NodeData
    {
        /// <summary>
        /// Gets or sets the name for this data entry.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public object Value { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZWaveLib.NodeData"/> class.
        /// </summary>
        /// <param name="fieldName">Field name.</param>
        /// <param name="data">Data.</param>
        public NodeData(string fieldName, object data)
        {
            Name = fieldName;
            Value = data;
        }
    }

    /// <summary>
    /// Z-wave node object.
    /// </summary>
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
        public byte Id { get; /*protected*/ set; }

        public NodeCapabilities ProtocolInfo { get; /*internal*/ set; }

        public NodeVersion Version { get; /*internal*/ set; }

        /// <summary>
        /// Gets or sets the node information frame.
        /// </summary>
        /// <value>The node information frame.</value>
        public byte[] NodeInformationFrame { get; /*internal*/ set; }

        /// <summary>
        /// Gets or sets the secured node information frame.
        /// </summary>
        /// <value>The secured node information frame.</value>
        public byte[] SecuredNodeInformationFrame { get; /*internal*/ set; }

        public List<NodeCommandClass> CommandClasses { get; /*internal*/ set; }

        /// <summary>
        /// Gets or sets the manufacturer specific.
        /// </summary>
        /// <value>The manufacturer specific.</value>
        public ManufacturerSpecificInfo ManufacturerSpecific { get; /*internal*/ set; }

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

        /// <summary>
        /// Initializes a new instance of the <see cref="ZWaveLib.ZWaveNode"/> class.
        /// </summary>
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

        internal virtual bool ApplicationCommandHandler(byte[] rawMessage)
        {
            NodeEvent messageEvent = null;
            int messageLength = rawMessage.Length;

            if (messageLength > 8)
            {
                byte commandLength = rawMessage[6];
                byte commandClass = rawMessage[7];
                // TODO: this should be moved inside the NodeCommandClass class
                // TODO: as "Instance" property
                var cc = CommandClassFactory.GetCommandClass(commandClass);
                byte[] message = new byte[commandLength];
                Array.Copy(rawMessage, 7, message, 0, commandLength);
                try
                {
                    if (cc != null)
                    {
                        messageEvent = cc.GetEvent(this, message);
                    }else
                    {
                        Utility.logger.Debug("CommandClass {0} not supported yet", commandClass);
                    }
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
            else if (messageLength > 3 && rawMessage[3] != (byte)ZWaveFunction.SendData)
            {
                Utility.logger.Warn("Unhandled message type: {0}", BitConverter.ToString(rawMessage));
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
