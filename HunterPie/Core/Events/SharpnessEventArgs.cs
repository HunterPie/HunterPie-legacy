using System;
using HunterPie.Core.Enums;
using HunterPie.Core.Jobs;

namespace HunterPie.Core.Events
{

    public class SharpnessEventArgs : EventArgs
    {
        /// <summary>
        /// Current sharpness
        /// </summary>
        public int Sharpness { get; }

        /// <summary>
        /// Maximum sharpness available
        /// </summary>
        public int MaximumSharpness { get; }

        /// <summary>
        /// Sharpness level
        /// </summary>
        public SharpnessLevel Level { get; }

        /// <summary>
        /// Maximum sharpness for that level
        /// </summary>
        public int Max { get; }

        /// <summary>
        /// Minimum sharpness for that level
        /// </summary>
        public int Min { get; }

        /// <summary>
        /// Array with all sharpness levels values
        /// </summary>
        public short[] SharpnessProgress { get; }

        public SharpnessEventArgs(Job obj)
        {
            Sharpness = obj.Sharpness;
            Level = obj.SharpnessLevel;
            Max = obj.SharpnessMax;
            Min = obj.SharpnessMin;
            SharpnessProgress = obj.Sharpnesses;
            MaximumSharpness = obj.MaximumSharpness;
        }
    }
}
