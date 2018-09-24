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
using System.Linq;

namespace ZWaveLib.CommandClasses
{
    public class WakeUp : ICommandClass
    {
        public CommandClass GetClassId()
        {
            return CommandClass.WakeUp;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            NodeEvent nodeEvent = null;
            byte cmdType = message[1];
            switch (cmdType)
            {
            case (byte)Command.WakeUpIntervalReport:
                if (message.Length > 4)
                {
                    uint interval = ((uint)message[2]) << 16;
                    interval |= (((uint)message[3]) << 8);
                    interval |= (uint)message[4];
                    nodeEvent = new NodeEvent(node, EventParameter.WakeUpInterval, interval, 0);
                }
                break;
            case (byte)Command.WakeUpNotification:
                WakeUpNode(node);
                nodeEvent = new NodeEvent(node, EventParameter.WakeUpNotify, 1, 0);
                break;
            }
            return nodeEvent;
        }

        public static ZWaveMessage Get(ZWaveNode node)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.WakeUp, 
                (byte)Command.WakeUpIntervalGet 
            });
        }

        public static ZWaveMessage Set(ZWaveNode node, uint interval)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.WakeUp, 
                (byte)Command.WakeUpIntervalSet,
                (byte)((interval >> 16) & 0xff),
                (byte)((interval >> 8) & 0xff),
                (byte)((interval) & 0xff),
                0x01
            });
        }

        public static ZWaveMessage SendToSleep(ZWaveNode node)
        {
            ZWaveMessage msg = null;
            var wakeUpStatus = (WakeUpStatus)node.GetData("WakeUpStatus", new WakeUpStatus()).Value;
            if (!wakeUpStatus.IsSleeping)
            {
                // 0x01, 0x09, 0x00, 0x13, 0x2b, 0x02, 0x84, 0x08, 0x25, 0xee, 0x8b
                msg = node.SendDataRequest(new byte[] { 
                    (byte)CommandClass.WakeUp, 
                    (byte)Command.WakeUpNoMoreInfo,
                    0x25
                }).Wait();
                wakeUpStatus.IsSleeping = true;
                var nodeEvent = new NodeEvent(node, EventParameter.WakeUpSleepingStatus, 1 /* 1 = sleeping, 0 = awake */, 0);
                node.OnNodeUpdated(nodeEvent);
            }
            return msg;
        }

        public static void WakeUpNode(ZWaveNode node)
        {
            // If node was marked as sleeping, reset the flag
            var wakeUpStatus = node.GetData("WakeUpStatus");
            if (wakeUpStatus != null && wakeUpStatus.Value != null && ((WakeUpStatus)wakeUpStatus.Value).IsSleeping)
            {
                ((WakeUpStatus)wakeUpStatus.Value).IsSleeping = false;
                var wakeEvent = new NodeEvent(node, EventParameter.WakeUpSleepingStatus, 0 /* 1 = sleeping, 0 = awake */, 0);
                node.OnNodeUpdated(wakeEvent);
                // Resend queued messages while node was asleep
                var wakeUpResendQueue = GetResendQueueData(node);
                for (int m = 0; m < wakeUpResendQueue.Count; m++)
                {
                    Utility.logger.Trace("Sending message {0} {1}", m, BitConverter.ToString(wakeUpResendQueue[m]));
                    node.SendMessage(wakeUpResendQueue[m]);
                }
                wakeUpResendQueue.Clear();
            }
        }

        public static void ResendOnWakeUp(ZWaveNode node, byte[] msg)
        {
            int minCommandLength = 8;
            if (msg.Length >= minCommandLength && !(msg[6] == (byte)CommandClass.WakeUp && msg[7] == (byte)Command.WakeUpNoMoreInfo))
            {
                byte[] command = new byte[minCommandLength];
                Array.Copy(msg, 0, command, 0, minCommandLength);
                // discard any message having same header and command (first 8 bytes = header + command class + command)
                var wakeUpResendQueue = GetResendQueueData(node);
                for (int i = wakeUpResendQueue.Count - 1; i >= 0; i--)
                {
                    byte[] queuedCommand = new byte[minCommandLength];
                    Array.Copy(wakeUpResendQueue[i], 0, queuedCommand, 0, minCommandLength);
                    if (queuedCommand.SequenceEqual(command))
                    {
                        Utility.logger.Trace("Removing old message {0}", BitConverter.ToString(wakeUpResendQueue[i]));
                        try
                        {
                            wakeUpResendQueue.RemoveAt(i);
                        }
                        catch (Exception e)
                        {
                            Utility.logger.Warn("Error removing message from WakeUp queue: {0}", e.Message);
                        }
                    }
                }
                Utility.logger.Trace("Adding message {0}", BitConverter.ToString(msg));
                wakeUpResendQueue.Add(msg);
                var wakeUpStatus = (WakeUpStatus)node.GetData("WakeUpStatus", new WakeUpStatus()).Value;
                if (!wakeUpStatus.IsSleeping)
                {
                    wakeUpStatus.IsSleeping = true;
                    var nodeEvent = new NodeEvent(node, EventParameter.WakeUpSleepingStatus, 1 /* 1 = sleeping, 0 = awake */, 0);
                    node.OnNodeUpdated(nodeEvent);
                }
            }
        }

        public static bool GetAlwaysAwake(ZWaveNode node)
        {
            var alwaysAwake = node.GetData("WakeUpAlwaysAwake");
            if (alwaysAwake != null && alwaysAwake.Value != null && ((bool)alwaysAwake.Value) == true)
                return true;
            return false;
        }

        public static void SetAlwaysAwake(ZWaveNode node, bool alwaysAwake)
        {
            node.GetData("WakeUpAlwaysAwake", false).Value = alwaysAwake;
            if (alwaysAwake)
                WakeUpNode(node);
        }

        private static List<byte[]> GetResendQueueData(ZWaveNode node)
        {
            return (List<byte[]>)node.GetData("WakeUpResendQueue", new List<byte[]>()).Value;
        }
    }
}

