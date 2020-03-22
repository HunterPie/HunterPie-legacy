using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterPie.Core {
    public class Part {
        private float _Health { get; set; }
        private float _TotalHealth { get; set; }
        private byte _BrokenCounter { get; set; }
        private string MonsterID { get; set; }
        public Int64 PartAddress { get; set; } // So we don't need to re-scan the address everytime
        

        public int ID { get; set; } // Part index
        public string Name {
            get { return GStrings.GetMonsterPartByID(MonsterData.GetPartStringIDByPartIndex(MonsterID, ID)); } // TODO: GStrings
        }
        public byte BrokenCounter {
            get { return _BrokenCounter; } // TODO: Implement events
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
        public bool IsRemovable { get; private set; }
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

        public void SetPartInfo(string MonsterID, int ID, byte Counter, float Health, float TotalHealth) {
            this.MonsterID = MonsterID;
            this.ID = ID;
            this.TotalHealth = TotalHealth;
            this.BrokenCounter = Counter;
            this.Health = Health;
        }

        public override string ToString() {
            return $"Name: {this.Name} | ID: {this.ID} | HP: {this.Health}/{this.TotalHealth} | Counter: {this.BrokenCounter}";
        }

    }
}
