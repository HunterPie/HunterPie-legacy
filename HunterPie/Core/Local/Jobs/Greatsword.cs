using System;
using HunterPie.Logger;

namespace HunterPie.Core.Jobs
{
    public class GreatswordEventArgs : EventArgs
    {
        public uint ChargeLevel { get; }
        public float ChargeTimer { get; }

        public GreatswordEventArgs(Greatsword weapon)
        {
            ChargeLevel = weapon.ChargeLevel;
            ChargeTimer = weapon.ChargeTimer;
        }
    }
    public class Greatsword : Job
    {
        /// <summary>
        /// Focus doesn't affect the required charge time, only how fast the timer is incremented
        /// </summary>
        public static readonly float[] ChargeTimes = new float[]
        {
            0.78f,
            1.59f,
            2.38f,
            1f
        };

        private uint chargeLevel;
        private float chargeTimer;

        public uint ChargeLevel
        {
            get => chargeLevel;
            set
            {
                // The game sets the charge level to 2 after the level 3 for some reason
                if (chargeLevel == 3 && value == 2)
                {
                    return;
                }
                if (value != chargeLevel)
                {
                    chargeLevel = value;
                    Dispatch(OnChargeLevelChange);
                }
            }
        }

        public float ChargeTimer
        {
            get => chargeTimer;
            set
            {
                if (value != chargeTimer)
                {
                    chargeTimer = value;
                    Dispatch(OnChargeTimerChange);
                }
            }
        }

        public override int SafijiivaMaxHits => 5;

        public delegate void GreatswordEvents(object source, GreatswordEventArgs args);
        public event GreatswordEvents OnChargeLevelChange;
        public event GreatswordEvents OnChargeTimerChange;

        private void Dispatch(GreatswordEvents e) => e?.Invoke(this, new GreatswordEventArgs(this));
    }
}
