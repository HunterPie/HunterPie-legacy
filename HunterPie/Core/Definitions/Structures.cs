/*
    HunterPie structure definitions
*/

using System.Runtime.InteropServices;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using Formatting = Newtonsoft.Json.Formatting;

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
        public int unk1;
        public float ExtraMaxHealth;
        public float ExtraHealth;
        public int unk2;
        public int unk3;
        public int unk4;
        public int unk5;
        public int unk6;
        public int unk7;
        public long unk8;
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
        public int unk22;
        public int unk23;
        public int unk24;
        public int unk25;
        public int unk26;
        public int unk27;
        public int unk28;
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
        public int unk2;
        public int unk3;
        public int PartId;
        public int TenderizedCounter;
    }
}
