using System;
using HunterPie.Core.Definitions;
using HunterPie.Core.Local;

namespace HunterPie.Core.Events
{
    /// <summary>
    /// Event args related to the player stamina
    /// </summary>
    public class PlayerStaminaEventArgs : EventArgs
    {

        /// <summary>
        /// Player current stamina
        /// </summary>
        public float Stamina { get; }

        /// <summary>
        /// Player maximum stamina
        /// </summary>
        public float MaxStamina { get; }

        public float MaxPossibleStamina { get; }

        public sGuiStamina sGuiRawData { get; }

        public bool IsStaminaExtVisible { get; }

        public int SelectedItemId { get; }

        public PlayerStaminaEventArgs(StaminaComponent p)
        {
            Stamina = p.Stamina;
            MaxStamina = p.MaxStamina;
            sGuiRawData = p.sGuiRawData;
            IsStaminaExtVisible = p.IsStaminaExtVisible;
            SelectedItemId = p.SelectedItemId;
            MaxPossibleStamina = p.MaxPossibleStamina;
        }
    }
}
