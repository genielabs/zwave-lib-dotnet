using System.Collections.Generic;
using ZWaveLib.Values;

namespace ZWaveLib.CommandClasses.Irrigation
{
    public class IrrigationValveConfig
    {
        /// <summary>
        /// This field is used to indicate whether the valve to configure is the master valve or a zone valve.
        /// </summary>
        public bool UseMasterValve { get; set; }

        /// <summary>
        /// This field is used to indicate the Valve ID to configure.
        /// </summary>
        public int ValveId { get; set; }

        /// <summary>
        /// This field is used to configure the nominal current high threshold for the actual valve.
        /// This field MUST be expressed as a multiple of 10mA.
        /// </summary>
        /// <example>The value 23 represents 230 mA.</example>
        public byte NominalCurrentHighThreshold { get; set; }

        /// <summary>
        /// This field is used to configure the nominal current low threshold for the actual valve at the receiving node.
        /// This field MUST be expressed as a multiple of 10mA.
        /// </summary>
        /// <example>The value 23 represents 230 mA.</example>
        public byte NominalCurrentLowThreshold { get; set; }

        /// <summary>
        /// This field is used to configure the maximum allowed water flow for the specified valve.
        /// This field MUST be expressed in l/h (liter/hour).
        /// </summary>
        public double MaximumFlow { get; set; }

        /// <summary>
        /// This field is used to configure the flow high threshold for the specified valve.
        /// This field MUST be expressed in l/h (liter/hour).
        /// </summary>
        public double FlowHighThreshold { get; set; }

        /// <summary>
        /// This field is used to configure the flow low threshold for the specified valve.
        /// This field MUST be expressed in l/h (liter/hour).
        /// </summary>
        public double FlowLowThreshold { get; set; }

        /// <summary>
        /// This field is used to configure if the actual valve must turn off / close when the specified sensors are active.
        /// </summary>
        public IrrigationValveSensorUsageMask SensorUsage { get; set; }

        public byte[] ToByteArray()
        {
            var commandBytes = new List<byte>()
            {
                NominalCurrentHighThreshold,
                NominalCurrentLowThreshold
            };
            commandBytes.AddRange(ZWaveValue.GetValueBytes(MaximumFlow, 0x00));
            commandBytes.AddRange(ZWaveValue.GetValueBytes(FlowHighThreshold, 0x00));
            commandBytes.AddRange(ZWaveValue.GetValueBytes(FlowLowThreshold, 0x00));
            return commandBytes.ToArray();
        }

        internal static IrrigationValveConfig Parse(byte[] message)
        {
            var valveConfig = new IrrigationValveConfig
            {
                UseMasterValve = message[2] == 1,
                ValveId = message[3],
                NominalCurrentHighThreshold = message[4],
                NominalCurrentLowThreshold = message[5]
            };
            
            var maximumFlowValueOffset = 6;
            var maximumFlowValue = ZWaveValue.ExtractValueFromBytes(message, maximumFlowValueOffset + 1);
            valveConfig.MaximumFlow = maximumFlowValue.Value;

            var flowHighThresholdValueOffset = maximumFlowValueOffset + 1 + maximumFlowValue.Size;
            var flowHighThresholdValue = ZWaveValue.ExtractValueFromBytes(message, flowHighThresholdValueOffset + 1);
            valveConfig.FlowHighThreshold = flowHighThresholdValue.Value;

            var flowLowThresholdValueOffset = flowHighThresholdValueOffset + 1 + flowHighThresholdValue.Size;
            var flowLowThresholdValue = ZWaveValue.ExtractValueFromBytes(message, flowLowThresholdValueOffset + 1);
            valveConfig.FlowLowThreshold = flowLowThresholdValue.Value;

            var sensorUsageOffset = flowLowThresholdValueOffset + 1 + flowLowThresholdValue.Size;
            valveConfig.SensorUsage = (IrrigationValveSensorUsageMask) message[sensorUsageOffset];

            return valveConfig;
        }
    }
}
