using System;

namespace ZWaveLib.CommandClasses.Irrigation
{
    [Flags]
    public enum IrrigationValveSensorUsageMask
    {
        None = 0,

        /// <summary>
        /// Use Rain Sensor.
        /// The valve MUST turn off / close if rain is detected.
        /// A receiving node having no support for rain sensor MAY ignore this field.
        /// </summary>
        UseRainSensor = 1,

        /// <summary>
        /// Use Moisture Sensor.
        /// The valve MUST turn off / close if moisture is detected.
        /// A receiving node having no support for moisture sensor MAY ignore this field.
        /// </summary>
        UseMoistureSensor = 2
    }
}
