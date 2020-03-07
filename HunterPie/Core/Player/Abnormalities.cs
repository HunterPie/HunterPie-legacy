using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterPie.Core {
    public class Abnormalities {
        Dictionary<string, Abnormality> CurrentAbnormalities = new Dictionary<string, Abnormality>();

        public Abnormality this[string AbnormalityID] {
            get {
                if (CurrentAbnormalities.ContainsKey(AbnormalityID)) { return CurrentAbnormalities[AbnormalityID]; }
                else { return null; }
            }
        }

        #region Events
        public delegate void AbnormalitiesEvents(object source, AbnormalityEventArgs args);
        public event AbnormalitiesEvents OnNewAbnormality;

        protected virtual void _OnNewAbnormality(Abnormality abnorm) {
            OnNewAbnormality?.Invoke(this, new AbnormalityEventArgs(abnorm));
        }
        #endregion


        #region Methods
        public void Add(string AbnormId, Abnormality Abnorm) {
            CurrentAbnormalities.Add(AbnormId, Abnorm);
            _OnNewAbnormality(CurrentAbnormalities[AbnormId]);
        }

        public void Remove(string AbnormId) {
            CurrentAbnormalities[AbnormId].ResetDuration();
            CurrentAbnormalities.Remove(AbnormId);
        } 

        public void ClearAbnormalities() {
            foreach (string AbnormId in CurrentAbnormalities.Keys) {
                // Will trigger OnAbnormalityEnd event
                CurrentAbnormalities[AbnormId].ResetDuration();
            }
            CurrentAbnormalities.Clear();
        }
        #endregion
    }
}
