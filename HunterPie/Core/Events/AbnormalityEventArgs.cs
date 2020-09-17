using System;
using HunterPie.Core;

namespace HunterPie.Core.Events
{
    /// <summary>
    /// Event args for Abnormality events
    /// </summary>
    public class AbnormalityEventArgs : EventArgs
    {
        /// <summary>
        /// The abnormality object
        /// </summary>
        public Abnormality Abnormality { get; }

        public AbnormalityEventArgs(Abnormality abnorm)
        {
            Abnormality = abnorm;
        }
    }
}
