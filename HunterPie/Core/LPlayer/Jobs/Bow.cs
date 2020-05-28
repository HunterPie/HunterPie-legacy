using System;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class BowEventArgs : EventArgs
    {
        public float ChargeProgress { get; }
        public int ChargeLevel { get; }
        public int MaxChargeLevel { get; }

        public BowEventArgs(Bow weapon)
        {
            ChargeProgress = weapon.ChargeProgress;
            ChargeLevel = weapon.ChargeLevel;
            MaxChargeLevel = weapon.MaxChargeLevel;
        }
    }
    public class Bow : Job
    {
        private float chargeProgress;
        private int maxChargeLevel;
        private int chargeLevel;

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
        public int MaxChargeLevel
        {
            get => maxChargeLevel;
            set
            {
                if (value != maxChargeLevel)
                {
                    maxChargeLevel = value;
                    Dispatch(OnChargeLevelMaxUpdate);
                }
            }
        }
        public override int SafijiivaMaxHits => 8;

        public delegate void BowEvents(object source, BowEventArgs args);
        public event BowEvents OnChargeLevelChange;
        public event BowEvents OnChargeProgressUpdate;
        public event BowEvents OnChargeLevelMaxUpdate;

        private void Dispatch(BowEvents e) => e?.Invoke(this, new BowEventArgs(this));
    }
}
