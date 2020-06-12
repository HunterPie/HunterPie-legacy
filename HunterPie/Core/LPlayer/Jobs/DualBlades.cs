using System;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class DualBladesEventArgs : EventArgs
    {
        public bool InDemonMode { get; }
        public bool IsReducing { get; }
        public float DemonGauge { get; }

        public DualBladesEventArgs(DualBlades weapon)
        {
            InDemonMode = weapon.InDemonMode;
            DemonGauge = weapon.DemonGauge;
            IsReducing = weapon.IsReducing;
        }
    }
    public class DualBlades : Job
    {
        private bool inDemonMode;
        private bool isReducing;
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
        public bool IsReducing
        {
            get => isReducing;
            set
            {
                if (value != isReducing)
                {
                    isReducing = value;
                    Dispatch(OnDemonGaugeReduce);
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
        public event DualBladesEvents OnDemonGaugeReduce;

        private void Dispatch(DualBladesEvents e) => e?.Invoke(this, new DualBladesEventArgs(this));

    }
}
