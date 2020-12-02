using System;
using System.Collections.Generic;

namespace HunterPie.Core.Events
{
    /// <summary>
    /// Event args for the Monster spawn event
    /// </summary>
    public class MonsterSpawnEventArgs : EventArgs
    {
        /// <summary>
        /// Monster name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Monster em ID
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// Monster crown icon name
        /// </summary>
        public string Crown { get; }

        /// <summary>
        /// Monster current health
        /// </summary>
        public float Health { get; }

        /// <summary>
        /// Monster maximum health
        /// </summary>
        public float MaxHealth { get; }

        /// <summary>
        /// Whether this monster is the target or not
        /// </summary>
        public bool IsTarget { get; }

        /// <summary>
        /// Monster weaknesses. <br/>
        /// Key: Weakness icon name
        /// Value: Weakness level
        /// </summary>
        public Dictionary<string, int> Weaknesses { get; }

        public MonsterSpawnEventArgs(Monster monster)
        {
            Name = monster.Name;
            ID = monster.Id;
            Crown = monster.Crown;
            Health = monster.Health;
            MaxHealth = monster.MaxHealth;
            IsTarget = monster.IsTarget;
            Weaknesses = monster.Weaknesses;
        }
    }
}
