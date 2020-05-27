using System;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class JobEventArgs : EventArgs
    {
        public int SafijiivaRegenCounter { get; }
        public int SafijiivaMaxHits { get; }
        public bool IsWeaponSheathed { get; }

        public JobEventArgs(Job obj)
        {
            SafijiivaRegenCounter = obj.SafijiivaRegenCounter;
            SafijiivaMaxHits = obj.SafijiivaMaxHits;
            IsWeaponSheathed = obj.IsWeaponSheated;
        }
    }
    public abstract class Job
    {
        /*
            Basic information that every class has in common
        */

        private int safijiivaRegenCounter;
        private bool isWeaponSheathed;

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
        public bool IsWeaponSheated
        {
            get => isWeaponSheathed;
            set
            {
                if (value != isWeaponSheathed)
                {
                    isWeaponSheathed = value;
                    Dispatch(OnWeaponSheathStateChange);
                }
            }
        }

        public delegate void JobEvents(object source, JobEventArgs args);
        public event JobEvents OnSafijiivaCounterUpdate;
        public event JobEvents OnWeaponSheathStateChange;

        public void Dispatch(JobEvents e) => e?.Invoke(this, new JobEventArgs(this));
    }
}
