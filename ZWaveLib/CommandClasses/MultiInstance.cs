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
using System.Collections.Generic;
using ZWaveLib.Values;

namespace ZWaveLib.CommandClasses
{
    public class MultiInstance : ICommandClass
    {
        public CommandClass GetClassId()
        {
            return CommandClass.MultiInstance;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            NodeEvent nodeEvent = null;

            //byte cmdClass = message[0];
            byte cmdType = message[1];
            byte instanceCmdClass = message[2];

            switch (cmdType)
            {
                case (byte)Command.MultiChannelCapabilityReportV2:
                    nodeEvent = HandleMultiChannelCapabilityReportV2(node, message);
                    break;

            case (byte)Command.MultiInstanceEncapsulated:
                nodeEvent = HandleMultiInstanceEncapReport(node, message);
                break;

            //case (byte) Command.MultiInstanceReport:
            case (byte) Command.MultiChannelEncapsulated:
                nodeEvent = HandleMultiChannelEncapReport(node, message);
                //if (nodeEvent != null)
                //{
                //    nodeEvent.Instance = (int) message[2];
                //}
                break;

            case (byte) Command.MultiInstanceCountReport:
                byte instanceCount = message[3];
                switch (instanceCmdClass)
                {
                case (byte) CommandClass.SwitchBinary:
                    nodeEvent = new NodeEvent(node, EventParameter.MultiinstanceSwitchBinaryCount, instanceCount, 0);
                    break;
                case (byte) CommandClass.SwitchMultilevel:
                    nodeEvent = new NodeEvent(node, EventParameter.MultiinstanceSwitchMultilevelCount, instanceCount, 0);
                    break;
                case (byte) CommandClass.SensorBinary:
                    nodeEvent = new NodeEvent(node, EventParameter.MultiinstanceSensorBinaryCount, instanceCount, 0);
                    break;
                case (byte) CommandClass.SensorMultilevel:
                    nodeEvent = new NodeEvent(node, EventParameter.MultiinstanceSensorMultilevelCount, instanceCount, 0);
                    break;
                }
                break;

            }

            return nodeEvent;
        }

        public static ZWaveMessage GetCount(ZWaveNode node, byte commandClass)
        {
            return node.SendDataRequest(new byte[] {
                (byte)CommandClass.MultiInstance,
                (byte)Command.MultiInstanceCountGet,
                commandClass
            });
        }

        public static ZWaveMessage SwitchBinaryGet(ZWaveNode node, byte instance)
        {
            return node.SendDataRequest(new byte[] {
                (byte)CommandClass.MultiInstance,
                0x0d, // ?? (MultiInstaceV2Encapsulated ??)
                0x00, // ??
                instance,
                (byte)CommandClass.SwitchBinary,
                (byte)Command.MultiInstanceGet
            });
        }

        public static ZWaveMessage SwitchBinarySet(ZWaveNode node, byte instance, int value)
        {
            return node.SendDataRequest(new byte[] {
                (byte)CommandClass.MultiInstance,
                0x0d, //  ?? (MultiInstaceV2Encapsulated ??)
                0x00, // ??
                instance,
                (byte)CommandClass.SwitchBinary,
                (byte)Command.MultiInstanceSet,
                byte.Parse(value.ToString())
            });
        }

        public static ZWaveMessage SwitchMultiLevelGet(ZWaveNode node, byte instance)
        {
            return node.SendDataRequest(new byte[] {
                (byte)CommandClass.MultiInstance,
                0x0d, // ?? (MultiInstaceV2Encapsulated ??)
                0x00, // ??
                instance,
                (byte)CommandClass.SwitchMultilevel,
                (byte)Command.MultiInstanceGet
            });
        }

        public static ZWaveMessage SwitchMultiLevelSet(ZWaveNode node, byte instance, int value)
        {
            return node.SendDataRequest(new byte[] {
                (byte)CommandClass.MultiInstance,
                0x0d, // ?? (MultiInstaceV2Encapsulated ??)
                0x00, // ??
                instance,
                (byte)CommandClass.SwitchMultilevel,
                (byte)Command.MultiInstanceSet,
                byte.Parse(value.ToString())
            });
        }

        public static ZWaveMessage SensorBinaryGet(ZWaveNode node, byte instance)
        {
            return node.SendDataRequest(new byte[] {
                (byte)CommandClass.MultiInstance,
                0x06, // ??
                instance,
                (byte)CommandClass.SensorBinary,
                0x04 //
            });
        }

        public static ZWaveMessage SensorMultiLevelGet(ZWaveNode node, byte instance)
        {
            return node.SendDataRequest(new byte[] {
                (byte)CommandClass.MultiInstance,
                0x06, // ??
                instance,
                (byte)CommandClass.SensorMultilevel,
                0x04 //
            });
        }

        private NodeEvent HandleMultiChannelCapabilityReportV2 (ZWaveNode node, byte [] message)
        {
            if (message.Length < 5) {
                Utility.logger.Error (String.Format ("MultiChannel Capability Report message ERROR: message is too short: {0}", BitConverter.ToString (message)));
                return null;
            }

            bool isDynamic = (message [2] & 0x80) != 0;
            int endPoint = (message [2] & 0x7f);
            int genericDeviceClass = message [3];
            int specificDeviceClass = message [4];
            List<int> commandClasses = new List<int> ();
            for (int i = 5; i < message.Length; i++) {
                commandClasses.Add (message [i]);
            }

            var report = new CapabilityReport (isDynamic, endPoint, genericDeviceClass, specificDeviceClass, commandClasses.ToArray ());

            return new NodeEvent (node, EventParameter.CapabilityReport, report, 0);
        }

        private NodeEvent HandleMultiInstanceEncapReport(ZWaveNode node, byte[] message)
        {
            if (message.Length < 5)
            {
                Utility.logger.Error(String.Format("MultiInstance encapsulated message ERROR: message is too short: {0}", BitConverter.ToString(message)));
                return null;
            }

            byte instanceNumber = message[2];
            var instanceCmdClass = message[3];
            var instanceMessage = new byte[message.Length - 3]; //TODO:
            Array.Copy(message, 3, instanceMessage, 0, message.Length - 3);

            Utility.logger.Debug(String.Format("MultiInstance encapsulated message: CmdClass: {0}; message: {1}", instanceCmdClass, BitConverter.ToString(instanceMessage)));

            var cc = CommandClassFactory.GetCommandClass(instanceCmdClass);
            if (cc == null)
            {
                Utility.logger.Error(String.Format("Can't find CommandClass handler for command class {0}", instanceCmdClass));
                return null;
            }
            NodeEvent zevent = cc.GetEvent(node, instanceMessage);
            zevent.Instance = instanceNumber;
            zevent.NestedEvent = GetNestedEvent(instanceCmdClass, zevent);
            return zevent;
        }

        private NodeEvent HandleMultiChannelEncapReport(ZWaveNode node, byte[] message)
        {
            if (message.Length < 6)
            {
                Utility.logger.Error(String.Format("MultiChannel encapsulated message ERROR: message is too short: {0}", BitConverter.ToString(message)));
                return null;
            }

            var instanceNumber = message[2];
            var instanceCmdClass = message[4];
            var instanceMessage = new byte[message.Length - 4]; //TODO
            Array.Copy(message, 4, instanceMessage, 0, message.Length - 4);

            Utility.logger.Debug(String.Format("MultiChannel encapsulated message: CmdClass: {0}; message: {1}", instanceCmdClass, BitConverter.ToString(instanceMessage)));

            var cc = CommandClassFactory.GetCommandClass(instanceCmdClass);
            if (cc == null)
            {
                Utility.logger.Error(String.Format("Can't find CommandClass handler for command class {0}", instanceCmdClass));
                return null;
            }
            NodeEvent zevent = cc.GetEvent(node, instanceMessage);
            zevent.Instance = instanceNumber;
            zevent.NestedEvent = GetNestedEvent(instanceCmdClass, zevent);
            return zevent;
        }

        private NodeEvent GetNestedEvent(byte commandClass, NodeEvent nodeEvent)
        {
            NodeEvent nestedEvent = null;
            switch (commandClass)
            {
            case (byte) CommandClass.SwitchBinary:
                nestedEvent = new NodeEvent(nodeEvent.Node, EventParameter.MultiinstanceSwitchBinary, nodeEvent.Value, nodeEvent.Instance);
                break;
            case (byte) CommandClass.SwitchMultilevel:
                nestedEvent = new NodeEvent(nodeEvent.Node, EventParameter.MultiinstanceSwitchMultilevel, nodeEvent.Value, nodeEvent.Instance);
                break;
            case (byte) CommandClass.SensorBinary:
                nestedEvent = new NodeEvent(nodeEvent.Node, EventParameter.MultiinstanceSensorBinary, nodeEvent.Value, nodeEvent.Instance);
                break;
            case (byte) CommandClass.SensorMultilevel:
                nestedEvent = new NodeEvent(nodeEvent.Node, EventParameter.MultiinstanceSensorMultilevel, nodeEvent.Value, nodeEvent.Instance);
                break;
            }
            return nestedEvent;
        }
    }
}
