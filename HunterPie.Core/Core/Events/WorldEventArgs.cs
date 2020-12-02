using System;
using HunterPie.Core.Enums;

namespace HunterPie.Core.Events
{
    public class WorldEventArgs : EventArgs
    {
        /// <summary>
        /// Represents the world time
        /// </summary>
        public float WorldTime { get; }

        /// <summary>
        /// Representation of the day time
        /// </summary>
        public DayTime DayTime { get; }

        public WorldEventArgs(Game g)
        {
            WorldTime = g.WorldTime;
            DayTime = g.DayTime;
        }
    }
}
