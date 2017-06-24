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
