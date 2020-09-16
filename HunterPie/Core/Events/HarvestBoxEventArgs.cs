using System;
using HunterPie.Core.Local;

namespace HunterPie.Core.Events
{
    /// <summary>
    /// Event args for the Harvest Box events
    /// </summary>
    public class HarvestBoxEventArgs : EventArgs
    {
        /// <summary>
        /// Amount of items in the Harvest Box
        /// </summary>
        public int Counter { get; }
        /// <summary>
        /// Maximum amount of items the Harvest Box can hold
        /// </summary>
        public int Max { get; }

        public HarvestBoxEventArgs(HarvestBox harvestBox)
        {
            Counter = harvestBox.Counter;
            Max = harvestBox.Max;
        }
    }
}
