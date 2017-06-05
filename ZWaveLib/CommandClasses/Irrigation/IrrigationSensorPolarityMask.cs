using System;

namespace ZWaveLib.CommandClasses.Irrigation
{
    [Flags]
    public enum IrrigationSensorPolarityMask
    {
        None = 0,

        /// <summary>
        /// Rain Sensor Polarity (0 LOW, 1 HIGH)
        /// </summary>
        RainSensorPolarity = 1,

        /// <summary>
        /// Moisture Sensor Polarity (0 LOW, 1 HIGH)
        /// </summary>
        MoistureSensorPolarity = 2,

        /// <summary>
        /// This bit MUST be set to 1 to indicate that the other bits in the bitmask contain valid data.
        /// </summary>
        Valid = 128
    }
}
