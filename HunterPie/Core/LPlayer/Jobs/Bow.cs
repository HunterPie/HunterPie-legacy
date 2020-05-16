using System;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class BowEventArgs : EventArgs
    {
        public int ChargeLevel;

        public BowEventArgs(Bow weapon)
        {
            ChargeLevel = weapon.ChargeLevel;
        }
    }
    public class Bow : Job
    {
        private int chargeLevel;

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
        public override int SafijiivaMaxHits => 8;

        public delegate void BowEvents(object source, BowEventArgs args);
        public event BowEvents OnChargeLevelChange;

        private void Dispatch(BowEvents e) => e?.Invoke(this, new BowEventArgs(this));
    }
}
