using System;
using System.Linq;
using HunterPie.Core.Events;
using HunterPie.Core.Enums;

namespace HunterPie.Core.Jobs
{
    public abstract class Job
    {
        /*
            Basic information that every class has in common
        */
        private int safijiivaRegenCounter;
        private bool isWeaponSheathed;
        private int sharpness;
        private SharpnessLevel sharpnessLevel;

        public abstract Classes Type { get; }

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
        public abstract bool IsMelee { get; }
        public int Sharpness
        {
            get => sharpness;
            internal set
            {
                if (value != sharpness)
                {
                    MaximumSharpness = Math.Max(MaximumSharpness, value);
                    sharpness = value;
                    Dispatch(OnSharpnessChange);
                }
            }
        }
        public int MaximumSharpness { get; private set; }
        public SharpnessLevel SharpnessLevel
        {
            get => sharpnessLevel;
            internal set
            {
                if (value != sharpnessLevel)
                {
                    sharpnessLevel = value;
                    Dispatch(OnSharpnessLevelChange);
                }
            }
        }
        public SharpnessLevel MaxSharpnessLevel
        {
            get {
                for (int i = Sharpnesses.Length - 1; i > 0; i--)
                {
                    if (Sharpnesses[i] == 0)
                    {
                        return (SharpnessLevel)(i - 1);
                    }
                }
                return SharpnessLevel.Purple;
            }
        }
        public short[] Sharpnesses { get; internal set; }
        public int SharpnessMax
        {
            get => Sharpnesses.ElementAtOrDefault((int)SharpnessLevel) == 0 ?
                Sharpnesses.Last() : Sharpnesses.ElementAtOrDefault((int)SharpnessLevel);
        }
        public int SharpnessMin
        {
            get => SharpnessLevel == 0 ? 0 : Sharpnesses.ElementAtOrDefault((int)SharpnessLevel - 1);
        }

        public delegate void JobEvents(object source, JobEventArgs args);
        public delegate void SharpnessEvents(object source, SharpnessEventArgs args);
        public event JobEvents OnSafijiivaCounterUpdate;
        public event JobEvents OnWeaponSheathStateChange;

        public event SharpnessEvents OnSharpnessChange;
        public event SharpnessEvents OnSharpnessLevelChange;

        public void Dispatch(JobEvents e) => e?.Invoke(this, new JobEventArgs(this));
        public void Dispatch(SharpnessEvents e) => e?.Invoke(this, new SharpnessEventArgs(this));
    }
}
