using System;
using HunterPie.Core.Jobs;

namespace HunterPie.Core.Events
{
    public class JobEventArgs : EventArgs
    {
        /// <summary>
        /// Current hit counter for safi'jiiva regen. proc
        /// </summary>
        public int SafijiivaRegenCounter { get; }

        /// <summary>
        /// Total hits you need for the buff to proc
        /// </summary>
        public int SafijiivaMaxHits { get; }

        /// <summary>
        /// Whether the weapon is sheathed or not
        /// </summary>
        public bool IsWeaponSheathed { get; }

        public JobEventArgs(Job obj)
        {
            SafijiivaRegenCounter = obj.SafijiivaRegenCounter;
            SafijiivaMaxHits = obj.SafijiivaMaxHits;
            IsWeaponSheathed = obj.IsWeaponSheated;
        }
    }
}
