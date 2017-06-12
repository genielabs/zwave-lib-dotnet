using System;

namespace ZWaveLib.CommandClasses.Irrigation
{
    [Flags]
    public enum IrrigationSystemSensorStatusMask
    {
        None = 0,

        /// <summary>
        /// Flow Sensor currently active
        /// </summary>
        FlowSensorActive = 1,

        /// <summary>
        /// Pressure Sensor currently active
        /// </summary>
        PressureSensorActive = 2,

        /// <summary>
        /// Rain Sensor attached and active
        /// </summary>
        RainSensorActive = 4,

        /// <summary>
        /// Moisture Sensor attached and active
        /// </summary>
        MoistureSensorActive = 8
    }
}
