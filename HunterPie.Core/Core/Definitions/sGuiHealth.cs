using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    /// <summary>
    /// Structure for the HUD player health bar. It has a lot of other data,
    /// but it's irrelevant for HunterPie
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 0x60)]
    public struct sGuiHealth
    {
        public long cRef;
        public int unk0;
        public int unk1;
        public int unk2;
        public int unk3;
        public float Health;
        public int unk4;
        public long unkPtr;
        public int IsHoldingItemDisplay;
        public int unk5;
        public byte unk7; // probably padding
        public byte unk8; // probably padding
        public byte unk9; // probably padding
        public byte IsHealthExtVisible;
        public int unk10;
        public int unk11;
        public float MaxPossibleHealth;
        public float MaxHealth;
        public float MaxHealth2; // Not sure why there are two max healths
        public int ItemIdSelected;
        public int unk12;
        public long unkPtr2;
        public float HealthExtShowTimer;
        public float MaxHealthExtShowTimer;
    }
}
