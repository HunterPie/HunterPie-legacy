using System;
using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential, Size = 0x3C)]
    public struct sHealingData : IEquatable<sHealingData>
    {
        public ulong Ref1;
        public ulong Ref2;
        public float CurrentHeal;
        public float OldMaxHeal;
        public float MaxHeal;
        public float CurrentHealSpeed;
        public float MaxHealSpeed;
        public float unk;
        public float unk1;
        public int Stage; // Healing stage; 0 = not healing; 1 = first stage; 2 = second stage
        public int unk2;
        public int unk3;
        public int unk4;
        public int unk5;

        public bool Equals(sHealingData other)
        {
            return CurrentHeal == other.CurrentHeal && OldMaxHeal == other.OldMaxHeal &&
                MaxHeal == other.MaxHeal && CurrentHealSpeed == other.CurrentHealSpeed &&
                MaxHealSpeed == other.MaxHealSpeed && Stage == other.Stage;
        }
    }
}
