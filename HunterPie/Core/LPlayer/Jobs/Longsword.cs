using System;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class LongswordEventArgs : EventArgs
    {
        public float InnerGauge;
        public int ChargeLevel;
        public float OuterGauge;

        public LongswordEventArgs(Longsword weapon)
        {
            InnerGauge = weapon.InnerGauge;
            ChargeLevel = weapon.ChargeLevel;
            OuterGauge = weapon.OuterGauge;
        } 
    }
    public class Longsword
    {
        private float innerGauge;
        private int chargeLevel;
        private float outerGauge;

        public float InnerGauge
        {
            get => innerGauge;
            set
            {
                if (value != innerGauge)
                {
                    innerGauge = value;
                    Dispatch(OnInnerGaugeChange);
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
        public float OuterGauge
        {
            get => outerGauge;
            set
            {
                if (value != outerGauge)
                {
                    outerGauge = value;
                    Dispatch(OnOuterGaugeChange);
                }
            }
        }

        public delegate void LongswordEvents(object source, LongswordEventArgs args);
        public event LongswordEvents OnInnerGaugeChange;
        public event LongswordEvents OnChargeLevelChange;
        public event LongswordEvents OnOuterGaugeChange;

        private void Dispatch(LongswordEvents e)
        {
            e?.Invoke(this, new LongswordEventArgs(this));
        }

    }
}
