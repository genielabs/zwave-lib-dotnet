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
*     Author: https://github.com/mdave
*     Project Homepage: https://github.com/genielabs/zwave-lib-dotnet
*/

using System;
using ZWaveLib.Values;

namespace ZWaveLib.CommandClasses
{
    public class MultiCmd : ICommandClass
    {
        public CommandClass GetClassId()
        {
            return CommandClass.MultiCmd;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            byte i, offset = 3;

            NodeEvent parent = null, child = null;

            Utility.logger.Debug(String.Format("MultiCmd encapsulated message: {0}", BitConverter.ToString(message)));

            if (message[1] != (byte)1)
            {
                return parent;
            }

            // Loop over each message and process it in turn.
            for (i = 0; i < message[2]; i++)
            {
                // Length and command classes of sub-command.
                byte length = message[offset];
                byte cmdClass = message[offset + 1];

                // Copy message into new array.
                var instanceMessage = new byte[length];
                Array.Copy(message, offset + 1, instanceMessage, 0, length);
                Utility.logger.Debug(String.Format("Processing message chunk: {0}", BitConverter.ToString(instanceMessage)));

                // Move offset to the next encap message
                offset += (byte)(length + 1);

                // Grab command class from the factory. If we don't have one, print out a warning and continue.
                var cc = CommandClassFactory.GetCommandClass(cmdClass);
                if (cc == null)
                {
                    Utility.logger.Warn(String.Format("Can't find CommandClass handler for command class {0}", cmdClass));
                    continue;
                }

                // Chain this event onto previously seen events.
                NodeEvent tmp = cc.GetEvent(node, instanceMessage);
                if (tmp == null)
                {
                    continue;
                }

                if (parent == null)
                {
                    parent = child = tmp;
                }
                else
                {
                    child.NestedEvent = tmp;
                    child = tmp;
                }
            }

            return parent;
        }
    }
}
