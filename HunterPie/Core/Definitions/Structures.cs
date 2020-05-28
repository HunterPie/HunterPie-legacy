/*
    HunterPie structure definitions
*/

using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential, Size = 0xA0)]
    public struct sMonsterPart
    {
        public float MaxHealth;
        public float Health;
        public int unk0;
        public int Counter;
        int unk1;
        public float ExtraMaxHealth;
        public float ExtraHealth;
        int unk2;
        int unk3;
        int unk4;
        int unk5;
        int unk6;
        int unk7;
        int unk8;
        int unk9;
        int unk10;
        int unk11;
        int unk12;
        int unk13;
        int unk14;
        int unk15;
        int unk16;
        int unk17;
        int unk18;
        int unk19;
        int unk20;
        int unk21;
        int unk22;
        int unk23;
        int unk24;
        int unk25;
        int unk26;
        int unk27;
        int unk28;
        int unk29;
        public float ExtraTimer;
        public float ExtraMaxTimer;

        // TODO: Make a decent serializer LOL
        public override string ToString()
        {
            string s = "struct sMonsterPart {\n" +
                $"\tMaxHealth = {MaxHealth};\n" +
                $"\tHealth = {Health};\n" +
                $"\tunk0 = {unk0};\n" +
                $"\tCounter = {Counter};\n" +
                $"\tunk1 = {unk1};\n" +
                $"\tExtraMaxHealth = {ExtraMaxHealth};\n" +
                $"\tExtraHealth = {ExtraHealth};\n" +
                $"\tunk2 = {unk2};\n" +
                $"\tunk3 = {unk3};\n" +
                $"\tunk4 = {unk4};\n" +
                $"\tunk5 = {unk5};\n" +
                $"\tunk6 = {unk6};\n" +
                $"\tunk7 = {unk7};\n" +
                $"\tunk8 = {unk8};\n" +
                $"\tunk9 = {unk9};\n" +
                $"\tunk10 = {unk10};\n" +
                $"\tunk11 = {unk11};\n" +
                $"\tunk12 = {unk12};\n" +
                $"\tunk13 = {unk13};\n" +
                $"\tunk14 = {unk14};\n" +
                $"\tunk15 = {unk15};\n" +
                $"\tunk16 = {unk16};\n" +
                $"\tunk17 = {unk17};\n" +
                $"\tunk18 = {unk18};\n" +
                $"\tunk19 = {unk19};\n" +
                $"\tunk20 = {unk20};\n" +
                $"\tunk21 = {unk21};\n" +
                $"\tunk22 = {unk22};\n" +
                $"\tunk23 = {unk23};\n" +
                $"\tunk24 = {unk24};\n" +
                $"\tunk25 = {unk25};\n" +
                $"\tunk26 = {unk26};\n" +
                $"\tunk27 = {unk27};\n" +
                $"\tunk28 = {unk28};\n" +
                $"\tunk29 = {unk29};\n" +
                $"\tExtraTimer = {ExtraTimer};\n" +
                $"\tExtraMaxTimer = {ExtraMaxTimer};\n" +
                "}";
            return s;
        }

    }

    [StructLayout(LayoutKind.Sequential, Size = 0x34)]
    public struct sTenderizedPart
    {
        public float Timer;
        public float MaxTimer;
        public int TenderizedLevel;
        public long unk0;
        public float ExtraTimer;
        public float MaxExtraTimer;
        public int unk1;
        public int unk3;
        public int PartId;
        public int TenderizedCounter;
    }
}
