using System.Collections.Generic;
using System.Linq;

namespace HunterPie.Core
{
    public class Abnormalities
    {
        readonly Dictionary<string, Abnormality> CurrentAbnormalities = new Dictionary<string, Abnormality>();

        public Abnormality this[string AbnormalityID]
        {
            get
            {
                if (CurrentAbnormalities.ContainsKey(AbnormalityID)) { return CurrentAbnormalities[AbnormalityID]; }
                else { return null; }
            }
        }

        #region Events
        public delegate void AbnormalitiesEvents(object source, AbnormalityEventArgs args);
        public event AbnormalitiesEvents OnNewAbnormality;
        public event AbnormalitiesEvents OnAbnormalityRemove;

        protected virtual void _OnNewAbnormality(Abnormality abnorm) => OnNewAbnormality?.Invoke(this, new AbnormalityEventArgs(abnorm));

        protected virtual void _OnAbnormalityRemove(Abnormality abnorm) => OnAbnormalityRemove?.Invoke(this, new AbnormalityEventArgs(abnorm));
        #endregion

        #region Methods
        public void Add(string AbnormId, Abnormality Abnorm)
        {
            CurrentAbnormalities.Add(AbnormId, Abnorm);
            _OnNewAbnormality(CurrentAbnormalities[AbnormId]);
            CurrentAbnormalities[AbnormId].OnAbnormalityEnd += RemoveObsoleteAbnormality;
            // Logger.Debugger.Debug($"NEW ABNORMALITY: {Abnorm.Name} (ID: {Abnorm.Id})");
        }

        public void Remove(string AbnormId)
        {
            _OnAbnormalityRemove(CurrentAbnormalities[AbnormId]);
            CurrentAbnormalities[AbnormId].ResetDuration();
            CurrentAbnormalities.Remove(AbnormId);
        }

        public void ClearAbnormalities()
        {
            if (CurrentAbnormalities.Count == 0) return;

            string[] activeAbnormalities = CurrentAbnormalities.Keys.ToArray();
            foreach (string abnormId in activeAbnormalities)
            {
                // Will trigger OnAbnormalityEnd event
                Remove(abnormId);
            }
        }
        #endregion

        #region Abnormalities Management

        private void RemoveObsoleteAbnormality(object source, AbnormalityEventArgs args)
        {
            // Unhook event to release references
            args.Abnormality.OnAbnormalityEnd -= RemoveObsoleteAbnormality;
            // Remove abnormality
            Remove(args.Abnormality.InternalID);
            //Logger.Debugger.Log($"REMOVED ABNORMALITY: {args.Abnormality.Name}");
        }

        #endregion
    }
}
