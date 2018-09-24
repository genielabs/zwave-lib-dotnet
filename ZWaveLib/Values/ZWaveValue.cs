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
using System.Globalization;

namespace ZWaveLib.Values
{
    public class ZWaveValue
    {
        private const byte SizeMask = 0x07, 
            ScaleMask = 0x18, ScaleShift = 0x03, 
            PrecisionMask = 0xe0, PrecisionShift = 0x05;

        public double Value;
        public int Scale;
        public int Precision;
        public int Size = 1;

        public ZWaveValue()
        {
        }

        public ZWaveValue(double v, int precision, int scale, int size)
        {
            this.Value = v;
            this.Precision = precision;
            this.Scale = scale;
            this.Size = size;
        }

        /// <summary>
        /// Represents given precision, scale and size as one byte value
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte GetPrecisionScaleSize(int precision, int scale, int size)
        {
            return (byte)((precision << PrecisionShift) | (scale << ScaleShift) | size);
        }

        /// <summary>
        /// Get bytes representation of Z-Wave Value used in multilevel sensors.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte[] GetValueBytes(double value, int precision, int scale, int size)
        {
            List<byte> valueBytes = new List<byte>();
            valueBytes.Add(GetPrecisionScaleSize(precision, scale, size));
            int intValue = (int)(value * Math.Pow(10D, precision));
            int shift = (size - 1) << 3;
            for (int i = size; i > 0; --i, shift -= 8)
            {
                valueBytes.Add((byte)(intValue >> shift));
            }
            return valueBytes.ToArray();
        }

        /// <summary>
        /// Get bytes representation of Z-Wave Value used in multilevel sensors.
        /// Tries to guess the size and precision based on passed value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="scale"></param>
        /// <returns>PrecisionScaleSize byte and value bytes</returns>
        public static byte[] GetValueBytes(double value, byte scale)
        {
            // determine desired precision
            var stringValue = value.ToString(CultureInfo.InvariantCulture);
            var delimeterPosition = stringValue.IndexOf(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator, StringComparison.InvariantCulture);
            var precision = delimeterPosition < 0
                ? 0
                : stringValue.Substring(delimeterPosition + 1).Length;
            if(precision > 7) // we have only 3 bits to store precision
                throw new ArgumentOutOfRangeException();

            // determine desired size
            // handle cases when size is more than 4 bytes
            var doubleValue = value * Math.Pow(10D, precision);
            if (doubleValue > int.MaxValue)
                throw new ArgumentOutOfRangeException();

            var size = 4;
            if (doubleValue <= short.MaxValue)
                size = 2;
            if (doubleValue <= byte.MaxValue)
                size = 1;

            var pss = GetPrecisionScaleSize(precision, scale, size);
            var valueBytes = GetValueBytes(value, precision, scale, size);
            var msg = new List<byte>(pss);
            msg.AddRange(valueBytes);
            return msg.ToArray();
        }

        // adapted from:
        // https://github.com/dcuddeback/open-zwave/blob/master/cpp/src/command_classes/CommandClass.cpp#L289
        public static ZWaveValue ExtractValueFromBytes(byte[] message, int valueOffset)
        {
            ZWaveValue result = new ZWaveValue();
            try
            {
                byte size = (byte)(message[valueOffset - 1] & SizeMask);
                byte precision = (byte)((message[valueOffset - 1] & PrecisionMask) >> PrecisionShift);
                int scale = (int)((message[valueOffset - 1] & ScaleMask) >> ScaleShift);
                
                result.Size = size;
                result.Precision = precision;
                result.Scale = scale;
                
                int value = 0;
                byte i;
                for (i = 0; i < size; ++i)
                {
                    value <<= 8;
                    value |= (int)message[i + (int)valueOffset];
                }
                // Deal with sign extension. All values are signed
                if ((message[valueOffset] & 0x80) > 0)
                {
                    // MSB is signed
                    if (size == 1)
                    {
                        value = (int)((uint)value | 0xffffff00);
                    }
                    else if (size == 2)
                    {
                        value = (int)((uint)value | 0xffff0000);
                    }
                }
                //
                result.Value = ((double)value / (precision == 0 ? 1 : Math.Pow(10D, precision)));
            }
            catch
            {
                // TODO: report/handle exception
            }
            return result;
        }

    }
}

