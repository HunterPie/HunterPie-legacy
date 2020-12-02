using System;
using HunterPie.Core.Enums;

namespace HunterPie.Core.Jobs
{
    public class LongswordEventArgs : EventArgs
    {
        public float InnerGauge { get; }
        public int ChargeLevel { get; }
        public float OuterGauge { get; }
        public float HelmBreakerBlink { get; }
        public float IaiSlashBlink { get; }

        public LongswordEventArgs(Longsword weapon)
        {
            InnerGauge = weapon.InnerGauge;
            ChargeLevel = weapon.ChargeLevel;
            OuterGauge = weapon.OuterGauge;
            HelmBreakerBlink = weapon.HelmBreakerBlink;
            IaiSlashBlink = weapon.IaiSlashBlink;
        }
    }
    public class Longsword : Job
    {
        private float innerGauge;
        private int chargeLevel;
        private float outerGauge;
        private float helmBreakerBlink;
        private float iaiSlashBlink;

        public override int SafijiivaMaxHits => 6;
        public override Classes Type => Classes.LongSword;
        public override bool IsMelee => true;

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
        public float HelmBreakerBlink
        {
            get => helmBreakerBlink;
            set
            {
                if (value != helmBreakerBlink)
                {
                    helmBreakerBlink = value;
                    Dispatch(OnSpiritGaugeBlinkDurationUpdate);
                }
            }
        }
        public float IaiSlashBlink
        {
            get => iaiSlashBlink;
            set
            {
                if (value != iaiSlashBlink)
                {
                    iaiSlashBlink = value;
                    Dispatch(OnSpiritGaugeBlinkDurationUpdate);
                }
            }
        }

        public delegate void LongswordEvents(object source, LongswordEventArgs args);
        public event LongswordEvents OnInnerGaugeChange;
        public event LongswordEvents OnChargeLevelChange;
        public event LongswordEvents OnOuterGaugeChange;
        public event LongswordEvents OnSpiritGaugeBlinkDurationUpdate;

        private void Dispatch(LongswordEvents e) => e?.Invoke(this, new LongswordEventArgs(this));

    }
}
