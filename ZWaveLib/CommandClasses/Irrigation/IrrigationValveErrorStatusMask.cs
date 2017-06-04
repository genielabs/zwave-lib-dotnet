using System;

namespace ZWaveLib.CommandClasses.Irrigation
{
    [Flags]
    public enum IrrigationValveErrorStatusMask
    {
        None = 0,

        /// <summary>
        /// Short circuit has been detected.
        /// </summary>
        ShortCircuit = 1,

        /// <summary>
        /// Current high threshold has been detected.
        /// </summary>
        CurrentTooHigh = 2,

        /// <summary>
        /// Current low threshold has been detected.
        /// </summary>
        CurrentTooLow = 4,

        /// <summary>
        /// Maximum flow has been detected.
        /// </summary>
        MaximumFlow = 8,

        /// <summary>
        /// Flow high threshold has been detected.
        /// </summary>
        FlowTooHigh = 16,

        /// <summary>
        /// Flow low threshold has been detected.
        /// </summary>
        FlowTooLow = 32
    }
}
