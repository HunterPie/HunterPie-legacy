using System;
using HunterPie.Core.Definitions;
using HunterPie.Core.Local;

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

        /// <summary>
        /// Maximum health player can have
        /// </summary>
        public float MaxPossibleHealth { get; }

        /// <summary>
        /// raw HUD data
        /// </summary>
        public sGuiHealth sGuiRawData { get; }

        /// <summary>
        /// Whether the player is holding an item that can increase their maximum health
        /// </summary>
        public bool IsHealthExtVisible { get; }

        /// <summary>
        /// Currently selected item
        /// </summary>
        public int SelectedItemId { get; }

        public PlayerHealthEventArgs(HealthComponent p)
        {
            Health = p.Health;
            MaxHealth = p.MaxHealth;
            HealingData = p.HealHealth;
            RedHealth = p.RedHealth;
            MaxPossibleHealth = p.MaxPossibleHealth;
            sGuiRawData = p.sGuiRawData;
            IsHealthExtVisible = p.IsHealthExtVisible;
            SelectedItemId = p.SelectedItemId;
        }
    }
}
