using System;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class JobEventArgs : EventArgs
    {
        public int SafijiivaRegenCounter { get; }
        public int SafijiivaMaxHits { get; }

        public JobEventArgs(Job obj)
        {
            SafijiivaRegenCounter = obj.SafijiivaRegenCounter;
            SafijiivaMaxHits = obj.SafijiivaMaxHits;
        }
    }
    public abstract class Job
    {
        /*
            Basic information that every class has in common
        */

        private int safijiivaRegenCounter;

        public int SafijiivaRegenCounter
        {
            get => safijiivaRegenCounter;
            set
            {
                if (value != safijiivaRegenCounter)
                {
                    safijiivaRegenCounter = value;
                    Dispatch(OnSafijiivaCounterUpdate);
                }
            }
        }
        public abstract int SafijiivaMaxHits { get; }

        public delegate void JobEvents(object source, JobEventArgs args);
        protected virtual event JobEvents OnSafijiivaCounterUpdate;

        public void Dispatch(JobEvents e) => e?.Invoke(this, new JobEventArgs(this));
    }
}
