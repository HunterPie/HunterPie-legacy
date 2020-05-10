using System;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class SwitchAxeEventArgs : EventArgs
    {
        public float OuterGauge;
        public float InnerGauge;

        public SwitchAxeEventArgs(SwitchAxe weapon)
        {
            OuterGauge = weapon.OuterGauge;
            InnerGauge = weapon.InnerGauge;
        }
    }
    public class SwitchAxe
    {
        private float outerGauge;
        private float innerGauge;

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

        public delegate void SwitchAxeEvents(object source, SwitchAxeEventArgs args);
        public event SwitchAxeEvents OnOuterGaugeChange;
        public event SwitchAxeEvents OnInnerGaugeChange;

        private void Dispatch(SwitchAxeEvents e) => e?.Invoke(this, new SwitchAxeEventArgs(this));
    }
}
