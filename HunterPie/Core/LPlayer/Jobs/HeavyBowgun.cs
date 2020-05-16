using System;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class HeavyBowgunEventArgs : EventArgs
    {
        public float WyvernsnipeTimer;
        public float WyvernheartTimer;

        public HeavyBowgunEventArgs(HeavyBowgun weapon)
        {
            WyvernsnipeTimer = weapon.WyvernsnipeTimer;
            WyvernheartTimer = weapon.WyvernheartTimer;
        }
    }
    public class HeavyBowgun : Job
    {
        private float wyvernsnipeTimer;
        private float wyvernheartTimer;

        public float WyvernsnipeTimer
        {
            get => wyvernsnipeTimer;
            set
            {
                if (value != wyvernsnipeTimer)
                {
                    wyvernsnipeTimer = value;
                    Dispatch(OnWyvernsnipeUpdate);
                }
            }
        }
        public float WyvernheartTimer
        {
            get => wyvernheartTimer;
            set
            {
                if (value != wyvernheartTimer)
                {
                    wyvernheartTimer = value;
                    Dispatch(OnWyvernheartUpdate);
                }
            }
        }
        public override int SafijiivaMaxHits => 7;

        public delegate void HeavyBowgunEvents(object source, HeavyBowgunEventArgs args);
        public event HeavyBowgunEvents OnWyvernsnipeUpdate;
        public event HeavyBowgunEvents OnWyvernheartUpdate;

        private void Dispatch(HeavyBowgunEvents e) => e?.Invoke(this, new HeavyBowgunEventArgs(this));
    }
}
