using HunterPie.Core.Definitions;
using HunterPie.Core.Events;

namespace HunterPie.Core.Local
{
    /// <summary>
    /// Player health logic
    /// </summary>
    public class HealthComponent
    {
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

        #region Private
        private float health;
        private float maxHealth;
        private float redHealth;
        private sHealingData healHealth;

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
        #endregion
    }
}
