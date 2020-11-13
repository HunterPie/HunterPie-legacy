using System;
using HunterPie.Core.Definitions;

namespace HunterPie.Core.Events
{
    /// <summary>
    /// Event args related to the player health
    /// </summary>
    public class PlayerHealthEventArgs : EventArgs
    {

        /// <summary>
        /// Player current health
        /// </summary>
        public float Health { get; }

        /// <summary>
        /// Player maximum health
        /// </summary>
        public float MaxHealth { get; }

        /// <summary>
        /// Healing data
        /// </summary>
        public sHealingData HealingData { get; }

        /// <summary>
        /// Red health gauge
        /// </summary>
        public float RedHealth { get; }

        public PlayerHealthEventArgs(Player p)
        {
            Health = p.Health;
            MaxHealth = p.MaxHealth;
            HealingData = p.HealHealth;
            RedHealth = p.RedHealth;
        }
    }
}
