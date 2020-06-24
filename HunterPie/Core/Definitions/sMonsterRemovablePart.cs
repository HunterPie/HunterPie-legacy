using System;
using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential)]
    public struct sMonsterPartData : IEquatable<sMonsterPartData>
    {
        public long address;
        public float unk0; // Same as the normal part
        public float MaxHealth;
        public float Health;
        public int unk1; // value was 4 for Ruiner Nergigante's Horn
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

        public bool Equals(sMonsterPartData other)
        {
            return (address == other.address && unk0 == other.unk0 && MaxHealth == other.MaxHealth && Health == other.Health && Counter == other.Counter && unk2 == other.unk2);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct sUnk0MonsterRemovablePart : IEquatable<sUnk0MonsterRemovablePart>
    {
        public long address;
        public int unk0;
        public int unk1;

        public bool Equals(sUnk0MonsterRemovablePart other)
        {
            return address == other.address && unk0 == other.unk0 && unk1 == other.unk1;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct sUnk1MonsterRemovablePart : IEquatable<sUnk1MonsterRemovablePart>
    {
        public long address;
        public int unk0;
        public int unk1;
        public int unk2;
        public uint Index;
        public int unk4;
        public int unk5;

        public bool Equals(sUnk1MonsterRemovablePart other)
        {
            return address == other.address && unk0 == other.unk0 && unk1 == other.unk1 && unk2 == other.unk2 && Index == other.Index;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct sMonsterRemovablePart : IEquatable<sMonsterRemovablePart>
    {
        public sMonsterPartData Data;
        public int unk0;
        public int unk1;
        public sUnk0MonsterRemovablePart unk2;
        public sUnk1MonsterRemovablePart unk3;


        public bool Equals(sMonsterRemovablePart other)
        {
            return Data.Equals(other.Data) && unk2.Equals(other.unk2) & unk3.Equals(other.unk3) && unk0 == other.unk0 && unk1 == other.unk1;
        }
    }
}
