/*
    HunterPie structure definitions
*/

using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace HunterPie.Core.Definitions
{
    public class Helpers
    {
        public static string Serialize(object obj) {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x18)]
    public struct sHarvestBoxElement
    {
        public long unk0;
        public int ID;
        public int Amount;
    }

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
