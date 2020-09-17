using System;

namespace HunterPie.Core.Jobs
{
    public class HammerEventArgs : EventArgs
    {
        public bool IsPowerCharged { get; }
        public int ChargeLevel { get; }
        public float ChargeProgress { get; }

        public HammerEventArgs(Hammer weapon)
        {
            IsPowerCharged = weapon.IsPowerCharged;
            ChargeLevel = weapon.ChargeLevel;
            ChargeProgress = weapon.ChargeProgress;
        }
    }
    public class Hammer : Job
    {
        private bool isPowerCharged;
        private int chargeLevel;
        private float chargeProgress;

        public bool IsPowerCharged
        {
            get => isPowerCharged;
            set
            {
                if (value != isPowerCharged)
                {
                    isPowerCharged = value;
                    Dispatch(OnPowerChargeStateChange);
                }
            }
        }
        public int ChargeLevel
        {
            get => chargeLevel;
            set
            {
                if (value != chargeLevel)
                {
                    chargeLevel = value;
                    Dispatch(OnChargeLevelChange);
                }
            }
        }
        public float ChargeProgress
        {
            get => chargeProgress;
            set
            {
                if (value != chargeProgress)
                {
                    chargeProgress = value;
                    Dispatch(OnChargeProgressUpdate);
                }
            }
        }
        public override int SafijiivaMaxHits => 7;

        public delegate void HammerEvents(object source, HammerEventArgs args);
        public event HammerEvents OnPowerChargeStateChange;
        public event HammerEvents OnChargeLevelChange;
        public event HammerEvents OnChargeProgressUpdate;

        private void Dispatch(HammerEvents e) => e?.Invoke(this, new HammerEventArgs(this));
    }
}
