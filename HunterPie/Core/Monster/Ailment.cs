using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterPie.Core {
    class Ailment {
        float _Buildup { get; set; }
        float _Duration { get; set; }

        public int ID { get; set; }
        public float Buildup {
            get { return _Buildup; }
            set {
                // TODO: Implement event
            }
        }
        public float MaxBuildup { get; private set; }
        public float Duration {
            get { return _Duration; }
            set {
                // TODO: Implement event
            }
        }
        public float MaxDuration { get; private set; }

        //public void SetAilmentInfo(float ai_ID, )

    }
}
