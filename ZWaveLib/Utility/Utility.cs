/*
  This file is part of ZWaveLib (https://github.com/genielabs/zwave-lib-dotnet)

  Copyright (2012-2025) G-Labs (https://github.com/genielabs)

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
using System.Text.Json;
using SerialPortLib;

namespace ZWaveLib
{
    public static class Utility
    {

        internal static class Logger
        {
            public static void Info(String message, params object[] args)
            {
                Logging.LogInfo(message, Stringify(args));
            }

            public static void Warn(String message, params object[] args)
            {
                Logging.LogWarning(message, Stringify(args));
            }

            public static void Error(String message, params object[] args)
            {
                Logging.LogError(message, Stringify(args));
            }

            public static void Error(Exception exception)
            {
                Logging.LogError(exception);
            }

            public static void Debug(String message, params object[] args)
            {
                Logging.LogDebug(message, Stringify(args));
            }

            public static void Trace(String message, params object[] args)
            {
                Logging.LogTrace(message, Stringify(args));
            }

        }

        public static string Stringify(object[] args)
        {
            var s = "";
            foreach (var o in args)
            {
                s += (o is string ? o : JsonSerializer.Serialize(o)) + "\t";
            }
            return s.TrimEnd('\t');
        }

        //from
        //http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa
        public static string ByteArrayToHexString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }
        //from
        //http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa
        public static byte[] HexStringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static byte[] AppendByteToArray(byte[] byteArray, byte data)
        {
            int pos = -1;
            if (byteArray != null)
                pos = Array.IndexOf(byteArray, data);
            if (pos == -1)
            {
                if (byteArray != null)
                {
                    Array.Resize(ref byteArray, byteArray.Length + 1);
                }
                else
                {
                    byteArray = new byte[1];
                }
                byteArray[byteArray.Length - 1] = data;
            }
            return byteArray;
        }

        public static List<byte> ExtractNodesFromBitMask(byte[] receivedMessage)
        {
            var nodeList = new List<byte>();
            // Decode the nodes in the bitmask (byte 9 - 37)
            byte k = 1;
            for (int i = 7; i < 36; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    try
                    {
                        if ((receivedMessage[i] & ((byte)Math.Pow(2, j))) == ((byte)Math.Pow(2, j)))
                        {
                            nodeList.Add(k);
                        }
                    }
                    catch
                    {

                        System.Diagnostics.Debugger.Break();
                    }
                    k++;
                }
            }
            return nodeList;
        }

        public static byte[] ExtractRoutingFromBitMask(byte[] receivedMessage)
        {
            var routingInfo = new List<byte>();
            for (int by = 0; by < 29; by++)
            {
                for (int bi = 0; bi < 8; bi++)
                {
                    int result = receivedMessage[4 + by] & (0x01 << bi);
                    if (result > 0)
                    {
                        byte nodeRoute = (byte)((by << 3) + bi + 1);
                        routingInfo.Add(nodeRoute);
                    }
                }
            }
            return routingInfo.ToArray();
        }
    }
}

