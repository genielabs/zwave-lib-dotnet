using System;
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
        public byte ValveId { get; set; }

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
        /// Use Rain Sensor.
        /// The valve MUST turn off / close if rain is detected.
        /// A receiving node having no support for rain sensor MAY ignore this field.
        /// </summary>
        public bool UseRainSensor { get; set; }

        /// <summary>
        /// Use Moisture Sensor.
        /// The valve MUST turn off / close if moisture is detected.
        /// A receiving node having no support for moisture sensor MAY ignore this field.
        /// </summary>
        public bool UseMoistureSensor { get; set; }

        public byte[] ToByteArray()
        {
            var commandBytes = new List<byte>
            {
                Convert.ToByte(UseMasterValve),
                UseMasterValve ? (byte)1 : ValveId,
                NominalCurrentHighThreshold,
                NominalCurrentLowThreshold
            };
            commandBytes.AddRange(ZWaveValue.GetValueBytes(MaximumFlow, 0x00));
            commandBytes.AddRange(ZWaveValue.GetValueBytes(FlowHighThreshold, 0x00));
            commandBytes.AddRange(ZWaveValue.GetValueBytes(FlowLowThreshold, 0x00));
            commandBytes.Add((byte) (Convert.ToByte(UseRainSensor) + (Convert.ToByte(UseMoistureSensor) << 1)));
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
            var sensors = message[sensorUsageOffset];
            valveConfig.UseRainSensor = (sensors & 0x01) == 1;
            valveConfig.UseMoistureSensor = (sensors & 0x02) == 2;

            return valveConfig;
        }
    }
}
