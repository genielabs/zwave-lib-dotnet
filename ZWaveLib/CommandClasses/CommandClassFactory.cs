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
using System.Reflection;

namespace ZWaveLib.CommandClasses
{
    internal class CommandClassFactory
    {
        private static readonly object SyncLock = new object();
        private static Dictionary<byte, Type> _commandClasses;

        public static ICommandClass GetCommandClass(byte id)
        {
            if (_commandClasses == null)
            {
                lock (SyncLock)
                {
                    if (_commandClasses == null)
                    {
                        _commandClasses = CollectCommandClasses();
                    }
                }
            }

            Type type;
            if (!_commandClasses.TryGetValue(id, out type))
                return null;

            return (ICommandClass)Activator.CreateInstance(type);
        }

        private static Dictionary<byte, Type> CollectCommandClasses()
        {
            var commandClassTypes = new Dictionary<byte, Type>();
            var assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
            var types = assemblyTypes.Where(t => typeof(ICommandClass).IsAssignableFrom(t)).ToList();

            foreach (var type in types)
            {
                if (type == typeof(ICommandClass))
                    continue; // we are not going to use interface itself
                var cc = (ICommandClass)Activator.CreateInstance(type);
                var id = (byte)cc.GetClassId();
                commandClassTypes.Add(id, type);
            }

            return commandClassTypes;
        }
    }
}
