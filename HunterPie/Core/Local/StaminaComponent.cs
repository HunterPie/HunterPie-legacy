using System.Collections.Generic;
using HunterPie.Core.Definitions;
using HunterPie.Core.Events;

namespace HunterPie.Core.Local
{
    /// <summary>
    /// Player stamina logic
    /// </summary>
    public class StaminaComponent
    {

        public static readonly Dictionary<int, float> CanIncreaseMaxStamina = new Dictionary<int, float>()
        {
            // Ancient Potion
            { 4, 100 },
            // Energy Drink
            { 8, 25 },
            // Ration
            { 9, 25 },
            // Rare Steak
            { 10, 50 },
            // Well-Done Steak
            { 11, 50 },
            // Burnt Steak
            { 12, 25 },
            // Dash Juice
            { 18, 50 },
            
            // EZ Ration
            { 183, 25 },
            // EZ Max Potion
            { 185, 100 }
        };

        /// <summary>
        /// Player stamina
        /// </summary>
        public float Stamina
        {
            get => stamina;
            private set
            {
                if (value != stamina)
                {
                    stamina = value;
                    Dispatch(OnStaminaUpdate);
                }
            }
        }

        /// <summary>
        /// Player maximum stamina
        /// </summary>
        public float MaxStamina
        {
            get => maxStamina;
            private set
            {
                if (value != maxStamina)
                {
                    maxStamina = value;
                    Dispatch(OnMaxStaminaUpdate);
                }
            }
        }

        public sGuiStamina sGuiRawData { get; private set; }
        public float MaxPossibleStamina { get; private set; }

        public bool IsStaminaExtVisible { get; private set; }

        public int SelectedItemId
        {
            get => selectedItemId;
            private set
            {
                if (value != selectedItemId)
                {
                    selectedItemId = value;
                    Dispatch(OnStaminaExtStateUpdate);
                }
            }
        }

        public delegate void PlayerStaminaEvents(object source, PlayerStaminaEventArgs args);

        public event PlayerStaminaEvents OnStaminaUpdate;
        public event PlayerStaminaEvents OnMaxStaminaUpdate;
        public event PlayerStaminaEvents OnStaminaExtStateUpdate;

        #region Private
        private float stamina;
        private float maxStamina;
        private int selectedItemId;

        private void Dispatch(PlayerStaminaEvents e) => e?.Invoke(this, new PlayerStaminaEventArgs(this));

        /// <summary>
        /// Updates the stamina values
        /// </summary>
        /// <param name="maxStamina">current maximum stamina</param>
        /// <param name="stamina">current stamina</param>
        internal void Update(float maxStamina, float stamina)
        {
            MaxStamina = maxStamina;
            Stamina = stamina;
        }

        internal void Update(sGuiStamina guiData)
        {
            sGuiRawData = guiData;
            MaxPossibleStamina = guiData.maxPossibleStamina;

            IsStaminaExtVisible = CanIncreaseMaxStamina.ContainsKey(guiData.selectedItemId);
            SelectedItemId = guiData.selectedItemId;
        }
        #endregion
    }
}
