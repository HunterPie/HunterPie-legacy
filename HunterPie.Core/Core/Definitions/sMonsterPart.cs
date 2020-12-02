using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{

    [StructLayout(LayoutKind.Sequential)]
    public struct sUnkMonsterPart
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
        public int unk12;
        public int unk13;
        public int unk14;
        public int unk15;
        public int unk16;
        public int unk17;
        public int unk18;
        public int unk19;
        public int unk20;
        public int unk21;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct sMonsterPart
    {
        public sMonsterPartData Data;
        public sUnkMonsterPart unk9;
        public sUnkMonsterPart unk10;
        public sUnkMonsterPart unk11;
        public sUnkMonsterPart unk12;
        public int unk13;
        public int unk14;
        public int unk15;
        public int unk16;
        public int unk17;
        public int unk18;
        public int unk19;
        public int unk20;
        public int unk21;
        public int unk22;
        public int unk23;
        public int unk24;
        public int unk25;
        public int unk26;
    }
}
