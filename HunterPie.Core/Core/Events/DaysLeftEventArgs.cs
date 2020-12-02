using System;

namespace HunterPie.Core.Events
{
    /// <summary>
    /// Event args for the Argosy and Tailraiders events
    /// </summary>
    public class DaysLeftEventArgs : EventArgs
    {

        /// <summary>
        /// This depends on <b>Modifier</b> and on the event.<br/>
        /// <b>OnArgosyDaysChange -</b> If <b>Modifier</b> is true, this indicates the days left until argosy goes away from town.
        /// If it's false, it indicates how many days until Argosy comes back.<br/>
        /// </summary>
        public byte Days { get; }

        /// <summary>
        /// Depends on what event triggered it.<br/>
        /// <b>OnArgosyDaysChange -</b> Whether Argosy is in town or not.<br/>
        /// <b>OnTailraidersDaysChange -</b> If Tailraiders are deployed or not.
        /// </summary>
        public bool Modifier { get; }

        public DaysLeftEventArgs(byte days, bool modifier = false)
        {
            Days = days;
            Modifier = modifier;
        }
    }
}
