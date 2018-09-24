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
using System.Collections.Generic;

namespace ZWaveLib.CommandClasses.Irrigation
{
    /// <summary>
    /// SDS13740-1 Z-Wave Plus Device and Command Class Types and Defines Specification 2016-08-26
    /// Sigma Designs Inc.Types and Defines Page 126 of 460
    /// </summary>
    public class Irrigation : ICommandClass
    {
        public CommandClass GetClassId()
        {
            return CommandClass.Irrigation;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            NodeEvent nodeEvent = null;
            var cmdType = message[1];
            switch (cmdType)
            {
                case (byte)Command.IrrigationSystemInfoReport:
                    var value = new IrrigationSystemInfoReport
                    {
                        IsMasterValueSupported = message[2] == 1,
                        TotalNumberOfValves = message[3],
                        TotalNumberOfValveTables = message[4],
                        ValveTableMaxSize = message[5]
                    };
                    nodeEvent = new NodeEvent(node, EventParameter.IrrigationSystemInfoReport, value, 0);
                    break;

                case (byte)Command.IrrigationSystemStatusReport:
                    var systemStatus = IrrigationSystemStatusReport.Parse(message);
                    nodeEvent = new NodeEvent(node, EventParameter.IrrigationSystemStatusReport, systemStatus, 0);
                    break;

                case (byte)Command.IrrigationSystemConfigReport:
                    var systemConfig = IrrigationSystemConfig.Parse(message);
                    nodeEvent = new NodeEvent(node, EventParameter.IrrigationSystemConfigReport, systemConfig, 0);
                    break;

                case (byte)Command.IrrigationValveInfoReport:
                    var valveInfo = IrrigationValveInfo.Parse(message);
                    nodeEvent = new NodeEvent(node, EventParameter.IrrigationValveInfoReport, valveInfo, 0);
                    break;

                case (byte)Command.IrrigationValveConfigReport:
                    var valveConfig = IrrigationValveConfig.Parse(message);
                    nodeEvent = new NodeEvent(node, EventParameter.IrrigationValveConfigReport, valveConfig, 0);
                    break;

                case (byte)Command.IrrigationValveTableReport:
                    var valveTable = IrrigationValveTable.Parse(message);
                    nodeEvent = new NodeEvent(node, EventParameter.IrrigationValveTableReport, valveTable, 0);
                    break;
            }
            return nodeEvent;
        }

        /// <summary>
        /// This command is used to request a receiving node about its irrigation system information.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <remarks>SDS13781-2 4.42.4 Irrigation System Info Get Command</remarks>
        public static ZWaveMessage SystemInfoGet(ZWaveNode node)
        {
            return node.SendDataRequest(new[]
            {
                (byte) CommandClass.Irrigation,
                (byte) Command.IrrigationSystemInfoGet
            });
        }

        /// <summary>
        /// This command is used to request a receiving node about its irrigation system status.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <remarks>SDS13781-2 4.42.6 Irrigation System Status Get Command</remarks>
        public static ZWaveMessage SystemStatusGet(ZWaveNode node)
        {
            return node.SendDataRequest(new[]
            {
                (byte) CommandClass.Irrigation,
                (byte) Command.IrrigationSystemStatusGet
            });
        }

        /// <summary>
        /// This command allows the irrigation system to be configured accordingly.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="config"></param>
        /// <returns>SDS13781 4.42.8 Irrigation System Config Set Command</returns>
        public static ZWaveMessage SystemConfigSet(ZWaveNode node, IrrigationSystemConfig config)
        {
            var commandBytes = new List<byte>
            {
                (byte) CommandClass.Irrigation,
                (byte) Command.IrrigationSystemConfigSet
            };
            commandBytes.AddRange(config.ToByteArray());
            return node.SendDataRequest(commandBytes.ToArray());
        }

        /// <summary>
        /// This command is used to request a receiving node about its current irrigation system configuration.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <remarks>SDS13781 4.42.9 Irrigation System Config Get Command</remarks>
        public static ZWaveMessage SystemConfigGet(ZWaveNode node)
        {
            return node.SendDataRequest(new[]
            {
                (byte) CommandClass.Irrigation,
                (byte) Command.IrrigationSystemStatusGet
            });
        }

        /// <summary>
        /// This command is used to request general information about the specified valve.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="valveId">This field is used to indicate the Valve ID if the sending node requests the information about a zone valve.</param>
        /// <param name="useMasterValve">This field is used to indicate whether the sending node requests the information of the master valve or of a zone valve.</param>
        /// <returns></returns>
        /// <remarks>SDS13781 4.42.11 Irrigation Valve Info Get Command</remarks>
        public static ZWaveMessage ValveInfoGet(ZWaveNode node, byte valveId, bool useMasterValve = false)
        {
            var masterValveByte = Convert.ToByte(useMasterValve);
            if (useMasterValve)
                valveId = 1;

            return node.SendDataRequest(new[]
            {
                (byte) CommandClass.Irrigation,
                (byte) Command.IrrigationValveInfoGet,
                masterValveByte,
                valveId
            });
        }

        /// <summary>
        /// This command allows an irrigation valve to be configured accordingly.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        /// <remarks>SDS13781 4.42.13 Irrigation Valve Config Set Command</remarks>
        public static ZWaveMessage ValveConfigSet(ZWaveNode node, IrrigationValveConfig config)
        {
            var commandBytes = new List<byte>
            {
                (byte) CommandClass.Irrigation,
                (byte) Command.IrrigationValveConfigSet
            };
            commandBytes.AddRange(config.ToByteArray());

            return node.SendDataRequest(commandBytes.ToArray());
        }

        /// <summary>
        /// This command is used to request the current configuration of an irrigation valve.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="valveId"></param>
        /// <param name="useMasterValve"></param>
        /// <returns></returns>
        /// <remarks>SDS13781 4.42.14 Irrigation Valve Config Get Command</remarks>
        public static ZWaveMessage ValveConfigGet(ZWaveNode node, byte valveId, bool useMasterValve)
        {
            var masterValveByte = Convert.ToByte(useMasterValve);
            if (useMasterValve)
                valveId = 1;

            return node.SendDataRequest(new[]
            {
                (byte) CommandClass.Irrigation,
                (byte) Command.IrrigationValveConfigGet,
                masterValveByte,
                valveId
            });
        }

        /// <summary>
        /// The Irrigation Valve Run Command will run the specified valve for a specified duration.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="valveId">This field is used to specify the actual Valve ID.</param>
        /// <param name="useMasterValve">This field is used to indicate if the specified valve is the master valve or a zone valve.</param>
        /// <param name="duration">This field is used to specify the duration of the run in seconds.</param>
        /// <returns></returns>
        /// <remarks>SDS13781 4.42.16 Irrigation Valve Run Command</remarks>
        public static ZWaveMessage ValveRun(ZWaveNode node, byte valveId, bool useMasterValve, ushort duration)
        {
            var masterValveByte = Convert.ToByte(useMasterValve);
            if (useMasterValve)
                valveId = 1;
            var valueBytes = duration.ToBigEndianBytes();

            return node.SendDataRequest(new[]
            {
                (byte) CommandClass.Irrigation,
                (byte) Command.IrrigationValveRun,
                masterValveByte,
                valveId,
                valueBytes[0],
                valueBytes[1]
            });
        }

        /// <summary>
        /// This command is used to set a valve table with a list of valves and durations.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="valveTable"></param>
        /// <returns></returns>
        /// <remarks>SDS13781 4.42.17 Irrigation Valve Table Set Command</remarks>
        public static ZWaveMessage ValveTableSet(ZWaveNode node, IrrigationValveTable valveTable)
        {
            var commandBytes = new List<byte>
            {
                (byte) CommandClass.Irrigation,
                (byte) Command.IrrigationValveTableSet
            };
            commandBytes.AddRange(valveTable.ToByteArray());
            return node.SendDataRequest(commandBytes.ToArray());
        }

        /// <summary>
        /// This command is used to request the contents of the specified Valve Table ID.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="valveTableId">
        /// This field is used to specify the valve table ID.
        /// Valves tables MUST be identified sequentially from 1 to the total number available on the device.
        /// </param>
        /// <returns></returns>
        /// <remarks>SDS13781 4.42.17 Irrigation Valve Table Set Command</remarks>
        public static ZWaveMessage ValveTableGet(ZWaveNode node, byte valveTableId)
        {
            return node.SendDataRequest(new[]
            {
                (byte) CommandClass.Irrigation,
                (byte) Command.IrrigationValveTableGet,
                valveTableId
            });
        }

        /// <summary>
        /// This command is used to run the specified valve tables sequentially.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="valveTableIds">List of Valve Tables to run sequentially.</param>
        /// <returns></returns>
        /// <remarks>SDS13781 4.42.20 Irrigation Valve Table Run Command</remarks>
        public static ZWaveMessage ValveTableRun(ZWaveNode node, byte[] valveTableIds)
        {
            var commandBytes = new List<byte>
            {
                (byte) CommandClass.Irrigation,
                (byte) Command.IrrigationValveTableRun
            };
            commandBytes.AddRange(valveTableIds);
            return node.SendDataRequest(commandBytes.ToArray());
        }

        /// <summary>
        /// This command is used to prevent any irrigation activity triggered by the Schedule CC for a specified duration.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="duration">
        /// Duration of the system shutoff.
        /// Values in the range 1..254 indicate how many hours the irrigation system must stay shut off after reception of this command.
        /// The value 0 indicates to turn off any running valve (including the master valve) as well as cancel any active Irrigation Valve Table Run or Schedule.
        /// The value 255 indicates that the irrigation system MUST stay permanently shut off until the node receives special commands.
        /// </param>
        /// <returns></returns>
        /// <remarks>SDS13781 4.42.21 Irrigation System Shutoff Command</remarks>
        public static ZWaveMessage SystemShutoff(ZWaveNode node, byte duration)
        {
            return node.SendDataRequest(new[]
            {
                (byte) CommandClass.Irrigation,
                (byte) Command.IrrigationSystemShutoff,
                duration
            });
        }
    }
}
