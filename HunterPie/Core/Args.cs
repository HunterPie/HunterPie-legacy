using System;
using System.Collections.Generic;

namespace HunterPie.Core {
    public class FertilizerEventArgs : EventArgs {
        public int ID;
        public string Name;
        public int Amount;

        public FertilizerEventArgs(Fertilizer m) {
            this.ID = m.ID;
            this.Name = m.Name;
            this.Amount = m.Amount;
        }
    }

    public class HarvestBoxEventArgs : EventArgs {
        public int Counter;
        public int Max;

        public HarvestBoxEventArgs(HarvestBox m) {
            this.Counter = m.Counter;
            this.Max = m.Max;
        }

    }

    public class MonsterEventArgs : EventArgs {
        public string Name;
        public string ID;
        public float CurrentHP;
        public float TotalHP;
        public bool isTarget;
        public bool isEnraged;
        public Dictionary<string, int> Weaknesses;

        public MonsterEventArgs(Monster m) {
            this.Name = m.Name;
            this.ID = m.ID;
            this.CurrentHP = m.CurrentHP;
            this.TotalHP = m.TotalHP;
            this.isTarget = m.isTarget;
            this.Weaknesses = m.Weaknesses;
            this.isEnraged = m.isEnraged;
        }

    }
}
