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

    /* Monster Events */

    public class MonsterSpawnEventArgs : EventArgs {
        public string Name;
        public string ID;
        public float CurrentHP;
        public float TotalHP;
        public bool isTarget;
        public Dictionary<string, int> Weaknesses;

        public MonsterSpawnEventArgs(Monster m) {
            this.Name = m.Name;
            this.ID = m.ID;
            this.CurrentHP = m.CurrentHP;
            this.TotalHP = m.TotalHP;
            this.isTarget = m.isTarget;
            this.Weaknesses = m.Weaknesses;
        }
    }
    
    public class MonsterUpdateEventArgs : EventArgs {
        public float CurrentHP;

        public MonsterUpdateEventArgs(Monster m) {
            this.CurrentHP = m.CurrentHP;
        }
    }

}
