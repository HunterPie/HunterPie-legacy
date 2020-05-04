using System;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class GreatswordEventArgs : EventArgs
    {
        public uint ChargeLevel;

        public GreatswordEventArgs(Greatsword weapon)
        {
            ChargeLevel = weapon.ChargeLevel;
        }
    }
    public class Greatsword
    {
        private uint chargeLevel;

        public uint ChargeLevel
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

        public delegate void GreatswordEvents(object source, GreatswordEventArgs args);
        public event GreatswordEvents OnChargeLevelChange;

        private void Dispatch(GreatswordEvents e) => e?.Invoke(this, new GreatswordEventArgs(this));
    }
}
