using System;

namespace HunterPie.Core.Events
{
    /// <summary>
    /// Event args for the Monster ailment events
    /// </summary>
    public class MonsterAilmentEventArgs : EventArgs
    {
        /// <summary>
        /// Ailment name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Ailment duration
        /// </summary>
        public float Duration { get; }

        /// <summary>
        /// Ailment maximum duration
        /// </summary>
        public float MaxDuration { get; }

        /// <summary>
        /// Ailment buildup
        /// </summary>
        public float Buildup { get; }

        /// <summary>
        /// Ailment max buildup
        /// </summary>
        public float MaxBuildup { get; }

        /// <summary>
        /// Ailment counter of how many times this ailment has activated
        /// </summary>
        public uint Counter { get; }

        public MonsterAilmentEventArgs(Ailment ailment)
        {
            Name = ailment.Name;
            Duration = ailment.Duration;
            MaxDuration = ailment.MaxDuration;
            Buildup = ailment.Buildup;
            MaxBuildup = ailment.MaxBuildup;
            Counter = ailment.Counter;
        }
    }
}
