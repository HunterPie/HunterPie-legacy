using System;

namespace HunterPie.Core.Events
{
    /// <summary>
    /// Event args for the monster part events
    /// </summary>
    public class MonsterPartEventArgs : EventArgs
    {

        /// <summary>
        /// The monster who has this part
        /// </summary>
        public Monster Owner { get; }

        /// <summary>
        /// Part current health
        /// </summary>
        public float Health { get; }

        /// <summary>
        /// Part maximum health
        /// </summary>
        public float TotalHealth { get; }

        /// <summary>
        /// Whether this part has a special condition to be met before breaking
        /// </summary>
        public bool HasBreakConditions { get; }

        /// <summary>
        /// Part broken counter, increments whenever <b>Health</b> reaches 0
        /// </summary>
        public int BrokenCounter { get; }

        /// <summary>
        /// Part tenderize duration
        /// </summary>
        public float Duration { get; }

        /// <summary>
        /// Part tenderize maximum duration
        /// </summary>
        public float MaxDuration { get; }

        public MonsterPartEventArgs(Part part)
        {
            Owner = part.Owner;
            Health = part.Health;
            TotalHealth = part.TotalHealth;
            BrokenCounter = part.BrokenCounter;
            Duration = part.TenderizeDuration;
            MaxDuration = part.TenderizeMaxDuration;
            HasBreakConditions = part.HasBreakConditions;
        }
    }
}
