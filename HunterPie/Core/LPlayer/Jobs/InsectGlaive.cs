using System;

namespace HunterPie.Core.LPlayer.Jobs
{
    public enum KinsectChargeBuff
    {
        None,
        Yellow,
        Red,
        Both
    }
    public class InsectGlaiveEventArgs : EventArgs
    {
        public float RedBuff;
        public float WhiteBuff;
        public float OrangeBuff;
        public float KinsectStamina;
        public float RedKinsectTimer;
        public float YellowKinsectTimer;
        public KinsectChargeBuff KinsectChargeType;
        public int BuffQueueSize;
        public int FirstBuffQueued;
        public int SecondBuffQueued;

        public InsectGlaiveEventArgs(InsectGlaive weapon)
        {
            RedBuff = weapon.RedBuff;
            WhiteBuff = weapon.WhiteBuff;
            OrangeBuff = weapon.OrangeBuff;
            KinsectStamina = weapon.KinsectStamina;
            RedKinsectTimer = weapon.RedKinsectTimer;
            YellowKinsectTimer = weapon.YellowKinsectTimer;
            KinsectChargeType = weapon.KinsectChargeType;
            BuffQueueSize = weapon.BuffQueueSize;
            FirstBuffQueued = weapon.FirstBuffQueued;
            SecondBuffQueued = weapon.SecondBuffQueued;
        }
    }
    public class InsectGlaive : Job
    {
        private float redBuff;
        private float whiteBuff;
        private float orangeBuff;
        private float kinsectStamina;
        private float redKinsectTimer;
        private float yellowKinsectTimer;
        private KinsectChargeBuff kinsectChargeType;
        private int buffQueueSize;
        private int firstBuffQueued;
        private int secondBuffQueued;

        public float RedBuff
        {
            get => redBuff;
            set
            {
                if (value != redBuff)
                {
                    redBuff = Math.Max(0, value);
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
                    whiteBuff = Math.Max(0, value);
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
                    orangeBuff = Math.Max(0, value);
                    Dispatch(OnOrangeBuffUpdate);
                }
            }
        }
        public float KinsectStamina
        {
            get => kinsectStamina;
            set
            {
                if (value != kinsectStamina)
                {
                    kinsectStamina = Math.Max(0, value);
                    Dispatch(OnKinsectStaminaUpdate);
                }
            }
        }
        public float RedKinsectTimer
        {
            get => redKinsectTimer;
            set
            {
                if (value != redKinsectTimer)
                {
                    redKinsectTimer = Math.Max(0, value);
                    Dispatch(OnKinsectChargeBuffUpdate);
                }
            }
        }
        public float YellowKinsectTimer
        {
            get => yellowKinsectTimer;
            set
            {
                if (value != yellowKinsectTimer)
                {
                    yellowKinsectTimer = Math.Max(0, value);
                    Dispatch(OnKinsectChargeBuffUpdate);
                }
            }
        }
        public KinsectChargeBuff KinsectChargeType
        {
            get => kinsectChargeType;
            set
            {
                if (value != kinsectChargeType)
                {
                    kinsectChargeType = value;
                    Dispatch(OnKinsectChargeBuffChange);
                }
            }
        }
        public int BuffQueueSize
        {
            get => buffQueueSize;
            set
            {
                if (value != buffQueueSize)
                {
                    buffQueueSize = value;
                    Dispatch(BuffQueueChanged);
                }
            }
        }
        public int FirstBuffQueued
        {
            get => firstBuffQueued;
            set
            {
                if (value != firstBuffQueued)
                {
                    firstBuffQueued = value;
                    Dispatch(BuffQueueChanged);
                }
            }
        }
        public int SecondBuffQueued
        {
            get => secondBuffQueued;
            set
            {
                if (value != secondBuffQueued)
                {
                    secondBuffQueued = value;
                    Dispatch(BuffQueueChanged);
                }
            }
        }
        public override int SafijiivaMaxHits => 10;

        public delegate void InsectGlaiveEvents(object source, InsectGlaiveEventArgs args);
        public event InsectGlaiveEvents OnRedBuffUpdate;
        public event InsectGlaiveEvents OnWhiteBuffUpdate;
        public event InsectGlaiveEvents OnOrangeBuffUpdate;
        public event InsectGlaiveEvents OnKinsectStaminaUpdate;
        public event InsectGlaiveEvents OnKinsectChargeBuffUpdate;
        public event InsectGlaiveEvents OnKinsectChargeBuffChange;
        public event InsectGlaiveEvents BuffQueueChanged;

        private void Dispatch(InsectGlaiveEvents e) => e?.Invoke(this, new InsectGlaiveEventArgs(this));
    }
}
