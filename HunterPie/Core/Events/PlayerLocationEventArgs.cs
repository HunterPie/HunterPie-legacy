using System;

namespace HunterPie.Core.Events
{
    /// <summary>
    /// Event args for the player location events
    /// </summary>
    public class PlayerLocationEventArgs : EventArgs
    {
        /// <summary>
        /// Current zone name
        /// </summary>
        public string ZoneName { get; }

        /// <summary>
        /// Indicates whether the player is currently in a peace zone.
        /// A peace zone is where the player cannot use weapons.
        /// </summary>
        public bool InPeaceZone { get; }

        /// <summary>
        /// Indicates whether the player is currently in a Harvest Box zone.
        /// A Harvest Box zone is where the Harvest Box widget will be able to be visible.
        /// </summary>
        public bool InHarvestZone { get; }

        public PlayerLocationEventArgs(Player player)
        {
            ZoneName = player.ZoneName;
            InPeaceZone = player.InPeaceZone;
            InHarvestZone = player.InHarvestZone;
        }
    }
}
