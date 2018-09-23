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

using System.Collections.Generic;

namespace ZWaveLib.CommandClasses.Irrigation
{
    public class IrrigationValveTable
    {
        /// <summary>
        /// This field is used to specify the valve table ID.
        /// </summary>
        /// <remarks>This field MUST be set to a Valve Table ID supported by the receiving node.</remarks>
        public byte TableId { get; set; }

        /// <summary>
        /// These fields are used to specify valve IDs and their associated run duration.
        /// </summary>
        public List<IrrigationValveTableItem> Items { get; set; }

        public byte[] ToByteArray()
        {
            var array = new byte[1 + Items.Count * 3];
            array[0] = TableId;
            for (var i = 0; i < Items.Count; i++)
            {
                var tableItem = Items[i];
                var valueBytes = tableItem.Duration.ToBigEndianBytes();
                array[1 + i * 3] = tableItem.ValveId;
                array[1 + i * 3 + 1] = valueBytes[0];
                array[1 + i * 3 + 2] = valueBytes[1];
            }
            return array;
        }

        internal static IrrigationValveTable Parse(byte[] message)
        {
            var valveTable = new IrrigationValveTable
            {
                TableId = message[2],
                Items = new List<IrrigationValveTableItem>()
            };
            int i = 3;
            while (message.Length > i)
            {
                var tableItem = new IrrigationValveTableItem
                {
                    ValveId = message[i],
                    Duration = BitExtensions.FromBigEndianBytes(new[] {message[i + 1], message[i + 2]})
                };
                valveTable.Items.Add(tableItem);
                i += 3;
            }
            return valveTable;
        }
    }
}
