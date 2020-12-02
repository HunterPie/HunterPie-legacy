using System;

namespace HunterPie.Core.Events
{
    /// <summary>
    /// Event args for the Monster update event
    /// </summary>
    public class MonsterUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// Monster current health
        /// </summary>
        public float Health { get; }

        /// <summary>
        /// Monster maximum health
        /// </summary>
        public float MaxHealth { get; }

        /// <summary>
        /// Monster current stamina
        /// </summary>
        public float Stamina { get; }

        /// <summary>
        /// Monster maximum stamina
        /// </summary>
        public float MaxStamina { get; }

        /// <summary>
        /// Monster enrage timer
        /// </summary>
        public float Enrage { get; }

        /// <summary>
        /// Monster action name
        /// </summary>
        public string Action { get; }

        /// <summary>
        /// Monster raw action name
        /// </summary>
        public string RawAction { get; }

        /// <summary>
        /// Monster action id
        /// </summary>
        public int ActionId { get; }

        public MonsterUpdateEventArgs(Monster monster)
        {
            Health = monster.Health;
            MaxHealth = monster.MaxHealth;
            Enrage = monster.EnrageTimer;
            Stamina = monster.Stamina;
            MaxStamina = monster.MaxStamina;
            Action = monster.ActionName;
            RawAction = monster.ActionReferenceName;
            ActionId = monster.ActionId;
        }
    }
}
