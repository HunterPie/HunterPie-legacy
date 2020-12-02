using System;

namespace HunterPie.Core.Events
{
    /// <summary>
    /// Event args for the monster part events
    /// </summary>
    public class MonsterPartEventArgs : EventArgs
    {
        /// <summary>
        /// Part current health
        /// </summary>
        public float Health { get; }

        /// <summary>
        /// Part maximum health
        /// </summary>
        public float TotalHealth { get; }

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
            Health = part.Health;
            TotalHealth = part.TotalHealth;
            BrokenCounter = part.BrokenCounter;
            Duration = part.TenderizeDuration;
            MaxDuration = part.TenderizeMaxDuration;
        }
    }
}
