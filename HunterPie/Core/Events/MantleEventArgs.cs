using System;
using HunterPie.Core.Local;

namespace HunterPie.Core.Events
{
    /// <summary>
    /// Event args for the mantle events
    /// </summary>
    public class MantleEventArgs : EventArgs
    {
        /// <summary>
        /// Mantle name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Mantle game Id
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Mantle duration timer
        /// </summary>
        public float Timer { get; }

        /// <summary>
        /// Mantle maximum duration timer
        /// </summary>
        public float MaxTimer { get; }

        /// <summary>
        /// Mantle cooldown timer
        /// </summary>
        public float Cooldown { get; }

        /// <summary>
        /// Mantle maximum cooldown timer
        /// </summary>
        public float MaxCooldown { get; }

        public MantleEventArgs(Mantle mantle)
        {
            Name = mantle.Name;
            Id = mantle.ID;
            Timer = mantle.Timer;
            MaxTimer = mantle.staticTimer;
            Cooldown = mantle.Cooldown;
            MaxCooldown = mantle.staticCooldown;
        }
    }
}
