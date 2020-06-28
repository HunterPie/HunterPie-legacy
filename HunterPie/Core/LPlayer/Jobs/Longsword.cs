using System;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class LongswordEventArgs : EventArgs
    {
        public float InnerGauge { get; }
        public int ChargeLevel { get; }
        public float OuterGauge { get; }
        public float SpiritGaugeBlinkDuration { get; }

        public LongswordEventArgs(Longsword weapon)
        {
            InnerGauge = weapon.InnerGauge;
            ChargeLevel = weapon.ChargeLevel;
            OuterGauge = weapon.OuterGauge;
            SpiritGaugeBlinkDuration = weapon.SpiritGaugeBlinkDuration;
        }
    }
    public class Longsword : Job
    {
        private float innerGauge;
        private int chargeLevel;
        private float outerGauge;
        private float spiritGaugeBlinkDuration;

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
        public float SpiritGaugeBlinkDuration
        {
            get => spiritGaugeBlinkDuration;
            set
            {
                if (value != spiritGaugeBlinkDuration)
                {
                    spiritGaugeBlinkDuration = value;
                    Dispatch(OnSpiritGaugeBlinkDurationUpdate);
                }
            }
        }
        public override int SafijiivaMaxHits => 6;

        public delegate void LongswordEvents(object source, LongswordEventArgs args);
        public event LongswordEvents OnInnerGaugeChange;
        public event LongswordEvents OnChargeLevelChange;
        public event LongswordEvents OnOuterGaugeChange;
        public event LongswordEvents OnSpiritGaugeBlinkDurationUpdate;

        private void Dispatch(LongswordEvents e) => e?.Invoke(this, new LongswordEventArgs(this));

    }
}
