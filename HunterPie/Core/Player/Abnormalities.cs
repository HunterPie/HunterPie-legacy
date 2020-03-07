using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HunterPie.Core {
    public class Abnormalities {
        XmlDocument AbnormalitiesData;
        Dictionary<string, Abnormality> CurrentAbnormalities = new Dictionary<string, Abnormality>();

        public Abnormality this[string AbnormalityID] {
            get {
                if (CurrentAbnormalities.ContainsKey(AbnormalityID)) { return CurrentAbnormalities[AbnormalityID]; }
                else { return null; }
            }
        }

        #region Abnormalities Data
        public Abnormalities() {
            AbnormalitiesData = new XmlDocument();
            AbnormalitiesData.LoadXml(Properties.Resources.AbnormalityData);
        }

        public XmlNodeList GetHuntingHornAbnormalities() {
            XmlNodeList HuntingHornAbnormalitiesData = AbnormalitiesData.SelectNodes("//Abnormalities/HUNTINGHORN_Abnormalities/Abnormality");
            return HuntingHornAbnormalitiesData;
        }

        public XmlNodeList GetPalicoAbnormalities() {
            XmlNodeList PalicoAbnormalitiesData = AbnormalitiesData.SelectNodes("//Abnormalities/PALICO_Abnormalities/Abnormality");
            return PalicoAbnormalitiesData;
        }

        public XmlNodeList GetBlightAbnormalities() {
            XmlNodeList BlightAbnormalitiesData = AbnormalitiesData.SelectNodes("//Abnormalities/BLIGHT_Abnormalities/Abnormality");
            return BlightAbnormalitiesData;
        }

        public XmlNodeList GetMiscAbnormalities() {
            XmlNodeList BlightAbnormalitiesData = AbnormalitiesData.SelectNodes("//Abnormalities/MISC_Abnormalities/Abnormality");
            return BlightAbnormalitiesData;
        }

        #endregion

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
            CurrentAbnormalities[AbnormId].OnAbnormalityEnd += RemoveObsoleteAbnormality;
            Logger.Debugger.Log($"NEW ABNORMALITY: {Abnorm.Name} (ID: {Abnorm.ID})");
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

        #region Abnormalities Management

        private void RemoveObsoleteAbnormality(object source, AbnormalityEventArgs args) {
            // Unhook event to release references
            args.Abnormality.OnAbnormalityEnd -= RemoveObsoleteAbnormality;
            // Remove abnormality
            CurrentAbnormalities.Remove(args.Abnormality.InternalID);
            Logger.Debugger.Log($"REMOVED ABNORMALITY: {args.Abnormality.Name}");
        }

        #endregion
    }
}
