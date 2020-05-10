using System;
using System.Linq;

namespace HunterPie.Core {
    public class Ailment {
        float _Buildup { get; set; }
        float _Duration { get; set; }
        byte _Counter { get; set; }

        public Int64 Address { get; set; }
        public string Name {
            get { return GStrings.GetAilmentNameByID(MonsterData.AilmentsInfo.ElementAt(ID).Id); }
        }
        public int ID { get; set; }
        public float Buildup {
            get { return _Buildup; }
            set {
                if (value != _Buildup) {
                    _Buildup = value;
                    _OnBuildupChange();
                }
            }
        }
        public float MaxBuildup { get; private set; }
        public float Duration {
            get { return _Duration; }
            set {
                if (value != _Duration) {
                    _Duration = value;
                    _OnDurationChange();
                }
            }
        }
        public float MaxDuration { get; private set; }
        public byte Counter {
            get { return _Counter; }
            set {
                if (value != _Counter) {
                    _Counter = value;
                    _OnCounterChange();
                }
            }
        }
        #region Events
        public delegate void MonsterAilmentEvents(object source, MonsterAilmentEventArgs args);
        public event MonsterAilmentEvents OnBuildupChange;
        public event MonsterAilmentEvents OnDurationChange;
        public event MonsterAilmentEvents OnCounterChange;

        protected virtual void _OnBuildupChange() {
            OnBuildupChange?.Invoke(this, new MonsterAilmentEventArgs(this));
        }

        protected virtual void _OnDurationChange() {
            OnDurationChange?.Invoke(this, new MonsterAilmentEventArgs(this));
        }

        protected virtual void _OnCounterChange() {
            OnCounterChange?.Invoke(this, new MonsterAilmentEventArgs(this));
        }
        #endregion
        
        public void SetAilmentInfo(int ai_ID, float currentDuration, float maxDuration, float currentBuildup, float maxBuildup, byte counter) {
            ID = ai_ID;
            // Ailment duration
            MaxDuration = maxDuration;
            Duration = currentDuration;
            // Ailment buildup
            MaxBuildup = maxBuildup;
            Buildup = currentBuildup;
            // Counter
            Counter = counter;
        }


        public override string ToString() {
            return $"Ailment: {Name} ({ID}) | Duration: {Duration}/{MaxDuration} | Buildup: {Buildup}/{MaxBuildup} | {Counter}";
        }
    }
}
