using System;

namespace ZWaveLib
{
    /// <summary>Splits serial reads into complete Z-Wave frames across read boundaries.</summary>
    internal sealed class SerialFrameReader
    {
        // SOF and the one-byte length field precede the bytes counted by length.
        private readonly byte[] buffer = new byte[2 + byte.MaxValue];
        private int count;

        internal void Read(byte[] input, Action<byte[]> emit)
        {
            foreach (byte value in input)
            {
                if (count == 0 && value != (byte)FrameHeader.SOF)
                {
                    // ACK is consumed here. CAN and NAK go to the controller as before.
                    if (value != (byte)FrameHeader.ACK)
                        emit(new[] { value });
                    continue;
                }

                buffer[count++] = value;
                if (count < 2 || count < 2 + buffer[1])
                    continue;

                var frame = new byte[count];
                Array.Copy(buffer, frame, count);
                count = 0;
                emit(frame);
            }
        }
    }
}
