using System.Collections.Generic;
using HunterPie.Core.Definitions;
using HunterPie.Core.Events;

namespace HunterPie.Core.Local
{
    /// <summary>
    /// Player health logic
    /// </summary>
    public class HealthComponent
    {
        #region Static data

        public static readonly Dictionary<int, float> CanIncreaseMaxHealth = new Dictionary<int, float>()
        {
            // Max Potion
            { 3, 100 },
            // Ancient Potion
            { 4, 100 },
            // Nutrients
            { 14, 10 },
            // Mega Nutrients
            { 15, 20 },
            // EZ Max Potion
            { 185, 100 }
        };

        #endregion

        /// <summary>
        /// Player health
        /// </summary>
        public float Health
        {
            get => health;
            private set
            {
                if (health != value)
                {
                    health = value;
                    Dispatch(OnHealthUpdate);
                }
            }
        }

        /// <summary>
        /// Player maximum health
        /// </summary>
        public float MaxHealth
        {
            get => maxHealth;
            private set
            {
                if (value != maxHealth)
                {
                    maxHealth = value;
                    Dispatch(OnMaxHealthUpdate);
                }
            }
        }

        /// <summary>
        /// The red gauge in the player health bar
        /// </summary>
        public float RedHealth
        {
            get => redHealth;
            private set
            {
                if (redHealth != value)
                {
                    redHealth = value;
                    Dispatch(OnRedHealthUpdate);
                }
            }
        }

        /// <summary>
        /// The heal gauge in the player health bar
        /// </summary>
        public sHealingData HealHealth
        {
            get => healHealth;
            private set
            {
                if (!value.Equals(healHealth))
                {
                    healHealth = value;
                    Dispatch(OnHealHealth);
                }
            }
        }

        public sGuiHealth sGuiRawData { get; private set; }
        public float MaxPossibleHealth { get; private set; }

        public bool IsHealthExtVisible { get; private set; }

        public int SelectedItemId
        {
            get => selectedItemId;
            set
            {
                if (value != selectedItemId)
                {
                    selectedItemId = value;
                    Dispatch(OnHealthExtStateUpdate);
                }
            }
        }

        public delegate void PlayerHealthEvents(object source, PlayerHealthEventArgs args);

        /// <summary>
        /// Dispatched whenever the Health value changes
        /// </summary>
        public event PlayerHealthEvents OnHealthUpdate;

        /// <summary>
        /// Dispatched whenever the maximum health changes
        /// </summary>
        public event PlayerHealthEvents OnMaxHealthUpdate;

        /// <summary>
        /// Dispatched whenever the red health value changes
        /// </summary>
        public event PlayerHealthEvents OnRedHealthUpdate;

        /// <summary>
        /// Dispatched whenever the healing data changes
        /// </summary>
        public event PlayerHealthEvents OnHealHealth;

        /// <summary>
        /// Dispatched whenever the player is holding an item that can increase
        /// the player maximum health
        /// </summary>
        public event PlayerHealthEvents OnHealthExtStateUpdate;

        #region Private
        private float health;
        private float maxHealth;
        private float redHealth;
        private sHealingData healHealth;
        private int selectedItemId;

        private void Dispatch(PlayerHealthEvents e) => e?.Invoke(this, new PlayerHealthEventArgs(this));

        /// <summary>
        /// Updates health values
        /// </summary>
        /// <param name="maxHealth">Maximum Available Health</param>
        /// <param name="health">Current Health</param>
        /// <param name="healData">Healing Data</param>
        /// <param name="redHealth">Red Health</param>
        internal void Update(float maxHealth, float health, sHealingData healData, float redHealth)
        {
            MaxHealth = maxHealth;
            Health = health;
            HealHealth = healData;
            RedHealth = redHealth;
        }

        internal void Update(sGuiHealth guiData)
        {
            sGuiRawData = guiData;
            MaxPossibleHealth = guiData.MaxPossibleHealth;

            IsHealthExtVisible = CanIncreaseMaxHealth.ContainsKey(guiData.ItemIdSelected);
            SelectedItemId = guiData.ItemIdSelected;
        }
        #endregion
    }
}
