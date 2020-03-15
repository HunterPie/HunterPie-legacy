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

        public int ID { get; set; } // Part index
        public string Name {
            get { return null; } // TODO: GStrings
        }
        public byte BrokenCounter {
            get { return _BrokenCounter; } // TODO: Implement events
        }
        public float Health {
            get { return _Health; }
        }
        public float TotalHealth {
            get { return _TotalHealth; }
        }

        public delegate void MonsterPartEvents(object source, MonsterPartEventArgs args);
        public event MonsterPartEvents OnHealthChange;
        public event MonsterPartEvents OnBrokenCounterChange;

    }
}
