using System;

namespace HunterPie.Core.Events
{

    /// <summary>
    /// Event arguments for the Steam fuel event
    /// </summary>
    public class SteamFuelEventArgs : EventArgs
    {
        /// <summary>
        /// Available fuel
        /// </summary>
        public int Available { get; }

        /// <summary>
        /// Maximum fuel
        /// </summary>
        public int Max { get; }

        public SteamFuelEventArgs(int available, int max)
        {
            Available = available;
            Max = max;
        }
    }
}
