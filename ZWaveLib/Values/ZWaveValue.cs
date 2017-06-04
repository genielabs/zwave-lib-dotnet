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

