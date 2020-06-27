
using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential)]
    public struct sUnk0MonsterAilment
    {
        public long address;
        public int unk0;
        public int unk1;
        public int unk2;
        public int unk3;
        public int unk4;
        public int unk5;
        public int unk6;
        public int unk7;
        public int unk8;
        public int unk9;
        public int unk10;
        public int unk11;
        public long unk12;
        public long unk13;
        public long unk14;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct sMonsterAilment
    {
        public long Source; // Monster who has the ailment
        public int unk0;
        public int unk1;
        public uint Id;
        public float MaxDuration;
        public float unk3;
        public int unk4;
        public int unk5;
        public int unk6;
        public int unk7;
        public int unk8;
        public float Buildup;
        public int unk10;
        public float unk11;
        public float unk12;
        public float MaxBuildup;
        public float unk13;
        public float unk14;
        public float unk15;
        public float unk16;
        public float unk17;
        public float unk18;
        public int unk19;
        public float unk20;
        public float unk21;
        public int unk22;
        public int unk23;
        public int Duration;
        public int unk25;
        public uint Counter;
        public int unk27;
        public int unk28;
        public int unk29;
        public sUnk0MonsterAilment unk30;
        public int unk31;
        public int unk32; // ?????
        public int unk33;
        public int unk34;
        public int unk35;
        public int unk36;
        public int unk37;
        public int unk38;
    }
}
