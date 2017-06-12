namespace ZWaveLib.CommandClasses.Irrigation
{
    public class IrrigationSystemInfoReport
    {
        /// <summary>
        /// Indicate if a master valve is supported by the device.
        /// </summary>
        public bool IsMasterValueSupported { get; set; }

        /// <summary>
        /// Total number of zone valves supported by the device.
        /// </summary>
        public int TotalNumberOfValves { get; set; }

        /// <summary>
        /// Total number of valve tables that can be created/stored in the device.
        /// </summary>
        public int TotalNumberOfValveTables { get; set; }

        /// <summary>
        /// Maximum number of entries per valve table supported by the device.
        /// Must be in range 1..15.
        /// </summary>
        public int ValveTableMaxSize { get; set; }
    }
}
