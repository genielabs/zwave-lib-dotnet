using System.Collections.Generic;

namespace ZWaveLib.Values
{
    public sealed class NotificationSupportedReport
    {
        public bool SupportsLegacyAlarm { get; internal set; }
        public int[] Types { get; internal set; }

        // Bit zero is reserved in both the notification type and event masks.
        internal static int[] ReadMask(byte[] message, int start, int length)
        {
            if (message.Length < start + length)
                return null;
            var values = new List<int>();
            for (int bit = 1; bit < length * 8; bit++)
                if ((message[start + bit / 8] & (1 << (bit % 8))) != 0)
                    values.Add(bit);
            return values.ToArray();
        }
    }

    public sealed class NotificationEventSupportedReport
    {
        public byte Type { get; internal set; }
        public int[] Events { get; internal set; }
    }
}
