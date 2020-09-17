using System;
using HunterPie.Core;

namespace HunterPie.Core.Events
{
    /// <summary>
    /// Event args for the Fertilizer events
    /// </summary>
    public class FertilizerEventArgs : EventArgs
    {
        /// <summary>
        /// Fertilizer Id
        /// </summary>
        public int ID { get; }
        /// <summary>
        /// Fertilizer name
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Fertilizer amount
        /// </summary>
        public int Amount { get; }

        public FertilizerEventArgs(Fertilizer fertilizer)
        {
            ID = fertilizer.ID;
            Name = fertilizer.Name;
            Amount = fertilizer.Amount;
        }
    }
}
