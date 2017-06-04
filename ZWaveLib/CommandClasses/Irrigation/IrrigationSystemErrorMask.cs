using System;

namespace ZWaveLib.CommandClasses.Irrigation
{
    [Flags]
    public enum IrrigationSystemErrorMask
    {
        None = 0,

        /// <summary>
        /// The device has not been programmed
        /// </summary>
        NotProgrammed = 1,

        /// <summary>
        /// The device has experienced an emergency shutdown
        /// </summary>
        EmergencyShutdows = 2,

        /// <summary>
        /// The device’s pressure high threshold has been triggered
        /// </summary>
        PressureTooHigh = 4,

        /// <summary>
        /// The device’s pressure low threshold has been triggered
        /// </summary>
        PressureTooLow = 8,

        /// <summary>
        /// A valve or the master valve is reporting error
        /// </summary>
        ValveError = 16
    }
}
