using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class InsectGlaiveEventArgs : EventArgs
    {
        public float RedBuff;
        public float WhiteBuff;
        public float OrangeBuff;

        public InsectGlaiveEventArgs(InsectGlaive weapon)
        {
            RedBuff = weapon.RedBuff;
            WhiteBuff = weapon.WhiteBuff;
            OrangeBuff = weapon.OrangeBuff;
        }
    }
    public class InsectGlaive
    {
        private float redBuff;
        private float whiteBuff;
        private float orangeBuff;

        public float RedBuff
        {
            get => redBuff;
            set
            {
                if (value != redBuff)
                {
                    redBuff = value;
                    Dispatch(OnRedBuffUpdate);
                }
            }
        }
        public float WhiteBuff
        {
            get => whiteBuff;
            set
            {
                if (value != whiteBuff)
                {
                    whiteBuff = value;
                    Dispatch(OnWhiteBuffUpdate);
                }
            }
        }
        public float OrangeBuff
        {
            get => orangeBuff;
            set
            {
                if (value != orangeBuff)
                {
                    orangeBuff = value;
                    Dispatch(OnOrangeBuffUpdate);
                }
            }
        }

        public delegate void InsectGlaiveEvents(object source, InsectGlaiveEventArgs args);
        public event InsectGlaiveEvents OnRedBuffUpdate;
        public event InsectGlaiveEvents OnWhiteBuffUpdate;
        public event InsectGlaiveEvents OnOrangeBuffUpdate;

        private void Dispatch(InsectGlaiveEvents e) => e?.Invoke(this, new InsectGlaiveEventArgs(this));
    }
}
