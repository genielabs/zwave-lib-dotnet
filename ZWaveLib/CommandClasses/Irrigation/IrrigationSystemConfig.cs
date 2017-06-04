using System.Collections.Generic;
using ZWaveLib.Values;

namespace ZWaveLib.CommandClasses.Irrigation
{
    public class IrrigationSystemConfig
    {
        /// <summary>
        /// This field is used to configure a delay in seconds between turning on the master valve and turning on any “zone” valve.
        /// </summary>
        public byte MasterValveDelay { get; set; }

        /// <summary>
        /// These field is used to configure the pressure high threshold at the receiving node in kPa.
        /// </summary>
        public double PressureHighThreshold { get; set; }

        /// <summary>
        /// These field is used to configure the pressure low threshold at the receiving node in kPa.
        /// </summary>
        public double PressureLowThreshold { get; set; }

        /// <summary>
        /// This field is used to configure optional sensors’ polarity at the receiving node.
        /// </summary>
        public IrrigationSensorPolarityMask SensorPolarity { get; set; }

        
        public int MoistureSensorPolarity { get; set; }

        public byte[] ToByteArray()
        {
            var commandBytes = new List<byte> {MasterValveDelay};
            commandBytes.AddRange(ZWaveValue.GetValueBytes(PressureHighThreshold, 0x00));
            commandBytes.AddRange(ZWaveValue.GetValueBytes(PressureLowThreshold, 0x00));
            commandBytes.Add((byte) SensorPolarity);
            return commandBytes.ToArray();
        }

        internal static IrrigationSystemConfig Parse(byte[] message)
        {
            var systemconfig = new IrrigationSystemConfig
            {
                MasterValveDelay = message[2]
            };
            var highPressureThresholdValueOffset = 3;
            var highPressureThresholdValue = ZWaveValue.ExtractValueFromBytes(message, highPressureThresholdValueOffset + 1);
            systemconfig.PressureHighThreshold = highPressureThresholdValue.Value;

            var lowPressureThresholdValueOffset = highPressureThresholdValueOffset + 1 + highPressureThresholdValue.Size;
            var lowPressureThresholdValue = ZWaveValue.ExtractValueFromBytes(message, lowPressureThresholdValueOffset + 1);
            systemconfig.PressureLowThreshold = lowPressureThresholdValue.Value;

            var sensorPolarityOffset = lowPressureThresholdValueOffset + 1 + lowPressureThresholdValue.Size;
            systemconfig.SensorPolarity = (IrrigationSensorPolarityMask) message[sensorPolarityOffset];
            
            return systemconfig;
        }
    }
}
