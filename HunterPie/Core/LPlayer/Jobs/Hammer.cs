using System;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class HammerEventArgs : EventArgs
    {
        public bool IsPowerCharged;
        public int ChargeLevel;

        public HammerEventArgs(Hammer weapon)
        {
            IsPowerCharged = weapon.IsPowerCharged;
            ChargeLevel = weapon.ChargeLevel;
        }
    }
    public class Hammer : Job
    {
        private bool isPowerCharged;
        private int chargeLevel;

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
        public override int SafijiivaMaxHits => 7;

        public delegate void HammerEvents(object source, HammerEventArgs args);
        public event HammerEvents OnPowerChargeStateChange;
        public event HammerEvents OnChargeLevelChange;

        private void Dispatch(HammerEvents e) => e?.Invoke(this, new HammerEventArgs(this));
    }
}
