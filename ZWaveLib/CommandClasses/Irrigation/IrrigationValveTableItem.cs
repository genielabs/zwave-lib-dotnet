namespace ZWaveLib.CommandClasses.Irrigation
{
    public class IrrigationValveTableItem
    {
        public byte ValveId { get; set; }

        /// <summary>
        /// The duration in seconds.
        /// </summary>
        public ushort Duration { get; set; }
    }
}
