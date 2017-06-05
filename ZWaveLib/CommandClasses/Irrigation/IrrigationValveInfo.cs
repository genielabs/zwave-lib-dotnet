namespace ZWaveLib.CommandClasses.Irrigation
{
    public class IrrigationValveInfo
    {
        /// <summary>
        /// This field is used to indicate whether the sending node advertises the information of the master valve or of a zone valve.
        /// </summary>
        public bool IsMasterValve { get; set; }

        /// <summary>
        /// This field indicates if the actual valve is currently connected to the node or not.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// This field is used to indicate the Valve ID if the sending node requests the information about a zone valve.
        /// </summary>
        public int ValveId { get; set; }

        /// <summary>
        /// Valve’s nominal electric current when the valve is On / Open.
        /// This field MUST be expressed as a multiple of 10mA.
        /// </summary>
        public int NominalCurrent { get; set; }

        /// <summary>
        /// This bit mask provides valve error status fields.
        /// </summary>
        public IrrigationValveErrorStatusMask ErrorStatus { get; set; }

        private const byte IsMasterValueMask = 0x01;
        private const byte IsConnectedMask = 0x02;

        internal static IrrigationValveInfo Parse(byte[] message)
        {
            var valveInfo = new IrrigationValveInfo
            {
                IsMasterValve = (message[2] & IsMasterValueMask) == 1,
                IsConnected = (message[2] & IsConnectedMask) == 2,
                ValveId = message[3],
                NominalCurrent = message[4],
                ErrorStatus = (IrrigationValveErrorStatusMask)message[5]
            };
            return valveInfo;
        }
    }
}
