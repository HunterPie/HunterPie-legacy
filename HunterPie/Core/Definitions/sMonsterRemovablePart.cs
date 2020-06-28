using System;
using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{

    [StructLayout(LayoutKind.Sequential)]
    public struct sUnk0MonsterRemovablePart : IEquatable<sUnk0MonsterRemovablePart>
    {
        public long address;
        public int unk0;
        public int unk1;

        public bool Equals(sUnk0MonsterRemovablePart other) => address == other.address && unk0 == other.unk0 && unk1 == other.unk1;
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

        public bool Equals(sUnk1MonsterRemovablePart other) => address == other.address && unk0 == other.unk0 && unk1 == other.unk1 && unk2 == other.unk2 && Index == other.Index;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct sMonsterRemovablePart : IEquatable<sMonsterRemovablePart>
    {
        public sMonsterPartData Data;
        public int unk0;
        public int unk1;
        public sUnk0MonsterRemovablePart unk2;
        public sUnk1MonsterRemovablePart unk3;


        public bool Equals(sMonsterRemovablePart other) => Data.Equals(other.Data) && unk2.Equals(other.unk2) & unk3.Equals(other.unk3) && unk0 == other.unk0 && unk1 == other.unk1;
    }
}
