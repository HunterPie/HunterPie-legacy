using System;
using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential)]
    public struct sMonsterPartData : IEquatable<sMonsterPartData>
    {
        public long address;
        public float unk0;
        public float MaxHealth;
        public float Health;
        public int unk1;
        public int Counter;
        public int unk2;
        public float ExtraMaxHealth;
        public float ExtraHealth;
        public int unk3;
        public int unk4;
        public int unk5;
        public int unk6;
        public int unk7;
        public int unk8;

        public bool Equals(sMonsterPartData other) => (address == other.address && unk0 == other.unk0 && MaxHealth == other.MaxHealth && Health == other.Health && Counter == other.Counter && unk2 == other.unk2);
    }
}
