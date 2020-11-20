using System;
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

        public PlayerStaminaEventArgs(StaminaComponent p)
        {
            Stamina = p.Stamina;
            MaxStamina = p.MaxStamina;
        }
    }
}
