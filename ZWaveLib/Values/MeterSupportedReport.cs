using System.Collections.Generic;

namespace ZWaveLib.Values
{
    public sealed class MeterSupportedReport
    {
        public int MeterType { get; private set; }
        public int RateTypeMask { get; private set; }
        public bool SupportsReset { get; private set; }
        public int[] Scales { get; private set; }

        internal static MeterSupportedReport Parse(byte[] message, int version)
        {
            if (message.Length < 4)
                return null;
            bool extended = (version < 1 || version >= 4) && (message[3] & 0x80) != 0;
            if (extended && (message.Length < 5 || message.Length < 5 + message[4]))
                return null;

            var scales = new List<int>();
            int baseScaleCount = version == 2 ? 4 : version == 3 ? 8 : 7;
            for (int bit = 0; bit < baseScaleCount; bit++)
                if ((message[3] & (1 << bit)) != 0)
                    scales.Add(bit);
            if (extended)
            {
                for (int bit = 0; bit < message[4] * 8; bit++)
                    if ((message[5 + bit / 8] & (1 << (bit % 8))) != 0)
                        scales.Add(7 + bit);
            }
            return new MeterSupportedReport
            {
                MeterType = message[2] & 0x1f,
                RateTypeMask = version == 2 || version == 3 ? 0 : (message[2] >> 5) & 3,
                SupportsReset = (message[2] & 0x80) != 0,
                Scales = scales.ToArray()
            };
        }
    }
}
