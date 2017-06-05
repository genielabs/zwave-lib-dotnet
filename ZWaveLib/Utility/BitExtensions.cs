using System;

namespace ZWaveLib
{
    internal static class BitExtensions
    {
        internal static byte[] ToBigEndianBytes(this ushort value)
        {
            var valueBytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                valueBytes = new[] { valueBytes[1], valueBytes[0] };
            return valueBytes;
        }

        internal static ushort FromBigEndianBytes(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
                bytes = new[] {bytes[1], bytes[0]};

            return BitConverter.ToUInt16(bytes, 0);
        }
    }
}
