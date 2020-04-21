using System;

namespace HunterPie.Core {
    public class Part {
        private float _Health { get; set; }
        private float _TotalHealth { get; set; }
        private byte _BrokenCounter { get; set; }
        private int MonsterId { get; set; }
        public Int64 PartAddress { get; set; } // So we don't need to re-scan the address everytime
        

        public int Id { get; set; } // Part index
        public string Name {
            get { return GStrings.GetMonsterPartByID(MonsterData.MonstersInfo[MonsterId].Parts[Id].Id); }
        }
        public byte BrokenCounter {
            get { return _BrokenCounter; }
            set {
                if (value != _BrokenCounter) {
                    this._BrokenCounter = value;
                    _OnBrokenCounterChange();
                }
            }
        }
        public float Health {
            get { return _Health; }
            set {
                if (value != _Health) {
                    this._Health = value;
                    _OnHealthChange();
                }
            }
        }
        public float TotalHealth {
            get { return _TotalHealth; }
            set {
                if (value != _TotalHealth) {
                    this._TotalHealth = value;
                }
            }
        }
        public bool IsRemovable { get; set; }
        public string Group { get; set; }

        #region Events
        public delegate void MonsterPartEvents(object source, MonsterPartEventArgs args);
        public event MonsterPartEvents OnHealthChange;
        public event MonsterPartEvents OnBrokenCounterChange;

        protected virtual void _OnHealthChange() {
            OnHealthChange?.Invoke(this, new MonsterPartEventArgs(this));
        }

        protected virtual void _OnBrokenCounterChange() {
            OnBrokenCounterChange?.Invoke(this, new MonsterPartEventArgs(this));
        }
        #endregion

        public void SetPartInfo(int monsterId, int id, byte Counter, float Health, float TotalHealth) {
            this.MonsterId = monsterId;
            this.Id = id;
            this.TotalHealth = TotalHealth;
            this.BrokenCounter = Counter;
            this.Health = Health;
        }

        public override string ToString() {
            return $"Name: {this.Name} | ID: {this.Id} | HP: {this.Health}/{this.TotalHealth} | Counter: {this.BrokenCounter}";
        }

    }
}
