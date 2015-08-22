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

namespace ZWaveLib
{
    
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

        /// <summary>
        /// Gets or sets the manufacturer identifier.
        /// </summary>
        /// <value>The manufacturer identifier.</value>
        public string ManufacturerId { get; internal set; }

        /// <summary>
        /// Gets or sets the type identifier.
        /// </summary>
        /// <value>The type identifier.</value>
        public string TypeId { get; internal set; }

        /// <summary>
        /// Gets or sets the product identifier.
        /// </summary>
        /// <value>The product identifier.</value>
        public string ProductId { get; internal set; }

        /// <summary>
        /// Gets or sets the basic class.
        /// </summary>
        /// <value>The basic class.</value>
        public byte BasicClass { get; internal set; }

        /// <summary>
        /// Gets or sets the generic class.
        /// </summary>
        /// <value>The generic class.</value>
        public byte GenericClass { get; internal set; }

        /// <summary>
        /// Gets or sets the specific class.
        /// </summary>
        /// <value>The specific class.</value>
        public byte SpecificClass { get; internal set; }

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

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public Dictionary<string, object> Data { get; internal set; }

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
        /// <param name="controller">Controller.</param>
        /// <param name="nodeId">Node identifier.</param>
        public ZWaveNode(ZWaveController controller, byte nodeId)
        {
            this.controller = controller;
            Id = nodeId;
            Data = new Dictionary<string, object>();
            NodeInformationFrame = new byte[]{};
            SecuredNodeInformationFrame = new byte[]{};
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZWaveLib.ZWaveNode"/> class.
        /// </summary>
        /// <param name="controller">Controller.</param>
        /// <param name="nodeId">Node identifier.</param>
        /// <param name="genericType">Generic type.</param>
        public ZWaveNode(ZWaveController controller, byte nodeId, byte genericType)
        {
            this.controller = controller;
            Id = nodeId;
            GenericClass = genericType;
            Data = new Dictionary<string, object>();
            NodeInformationFrame = new byte[]{};
            SecuredNodeInformationFrame = new byte[]{};
        }

        #endregion

        #region Public members

        /// <summary>
        /// Gets the supported command classes.
        /// </summary>
        /// <value>The supported command classes.</value>
        public List<CommandClass> SupportedCommandClasses
        {
            get
            {
                var cclist = new List<CommandClass>();
                foreach (var cc in NodeInformationFrame)
                {
                    var cclass = CommandClass.NotSet;
                    Enum.TryParse<CommandClass>(cc.ToString(), out cclass);
                    cclist.Add(cclass);
                }
                return cclist;
            }
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
        /// Sends the data request.
        /// </summary>
        /// <param name="request">Request.</param>
        public void SendDataRequest(byte[] request)
        {
            byte cmdClass = request[0];
            byte[] message = ZWaveMessage.BuildSendDataRequest(Id, request);
            // when cmdClass belongs to SecuredNodeInformationFrame we need to encrypt the message
            if (cmdClass != (byte)CommandClass.Security && IsSecuredCommandClass((CommandClass)cmdClass))
            {
                Security.SendMessage(this, message);
            }
            else
            {
                SendMessage(message);
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
                /*
                if (receivedMessage[3] != 0x13)
                {
                    bool log = true;
                    // do not log an error message for ManufacturerSpecific and Security CommandClass
                    if (messageLength > 7 && (receivedMessage[7] == (byte)CommandClass.ManufacturerSpecific || receivedMessage[7] == (byte) CommandClass.Security))
                        log = false;
                    if (log)
                    {
                        Utility.DebugLog(DebugMessageType.Error, "Unhandled message: " + BitConverter.ToString(receivedMessage));
                    }
                }
                */
                Utility.logger.Warn("Unhandled message type");
            }

            return false;
        }

        internal bool SendMessage(byte[] message)
        {
            var msg = new ZWaveMessage(message);
            return controller.SendMessage(msg, true);
        }

        internal virtual void OnNodeUpdated(NodeEvent zevent)
        {
            if (NodeUpdated != null)
                NodeUpdated(this, zevent);
        }

        #endregion

    }
}
