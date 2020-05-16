using System;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class DualBladesEventArgs : EventArgs
    {
        public bool InDemonMode;
        public float DemonGauge;

        public DualBladesEventArgs(DualBlades weapon)
        {
            InDemonMode = weapon.InDemonMode;
            DemonGauge = weapon.DemonGauge;
        }
    }
    public class DualBlades : Job
    {
        private bool inDemonMode;
        private float demonGauge;

        public bool InDemonMode
        {
            get => inDemonMode;
            set
            {
                if (value != inDemonMode)
                {
                    inDemonMode = value;
                    Dispatch(OnDemonModeToggle);
                }
            }
        }
        public float DemonGauge
        {
            get => demonGauge;
            set
            {
                if (value != demonGauge)
                {
                    demonGauge = value;
                    Dispatch(OnDemonGaugeChange);
                }
            }
        }
        public override int SafijiivaMaxHits => 20;

        public delegate void DualBladesEvents(object source, DualBladesEventArgs args);
        public event DualBladesEvents OnDemonModeToggle;
        public event DualBladesEvents OnDemonGaugeChange;

        private void Dispatch(DualBladesEvents e) => e?.Invoke(this, new DualBladesEventArgs(this));

    }
}
