namespace HunterPie.Core {
    public class Abnormality {
        private int _Duration { get; set; }

        public string Name {
            get { return GStrings.GetAbnormalityByID(Type, ID, Stack); }
        }
        public string Type { get; private set; }
        public int ID { get; private set; }
        public byte Stack { get; private set; }
        public string InternalID { get; private set; }
        public bool IsInfinite { get; private set; }
        public int Duration {
            get { return _Duration; }
            set {
                if (value <= 0 && !IsInfinite) {
                    _OnAbnormalityEnd();
                    return;
                }
                if (value != _Duration) {
                    _Duration = value;
                    _OnAbnormalityUpdate();
                }
            }
        }
        public float MaxDuration { get; private set; }
        public bool IsBuff { get; private set; }

        #region Events

        public delegate void AbnormalityEvents(object source, AbnormalityEventArgs args);
        public event AbnormalityEvents OnAbnormalityStart;
        public event AbnormalityEvents OnAbnormalityUpdate;
        public event AbnormalityEvents OnAbnormalityEnd;

        protected virtual void _OnAbnormalityStart() {
            OnAbnormalityStart?.Invoke(this, new AbnormalityEventArgs(this));
        }

        protected virtual void _OnAbnormalityUpdate() {
            OnAbnormalityUpdate?.Invoke(this, new AbnormalityEventArgs(this));
        }

        protected virtual void _OnAbnormalityEnd() {
            OnAbnormalityEnd?.Invoke(this, new AbnormalityEventArgs(this));
        }

        #endregion

        #region Methods

        public void UpdateAbnormalityInfo(string Type, string InternalID,float ab_duration, byte ab_stack, int ab_id, bool ab_isBuff, bool IsInfinite) {
            this.Type = Type;
            this.InternalID = InternalID;
            this.MaxDuration = MaxDuration < ab_duration ? ab_duration : MaxDuration;
            this.Stack = (byte)(ab_stack);
            this.IsBuff = ab_isBuff;
            this.IsInfinite = IsInfinite;
            this.Duration = (int)ab_duration;
            this.ID = ab_id;
        }

        public void ResetDuration() {
            this.IsInfinite = false;
            this.Duration = 0;
        }

        #endregion
    }
}
