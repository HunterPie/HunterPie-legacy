using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    /// <summary>
    /// Structure for stamina gauge HUD
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct sGuiStamina
    {
        public long cRef;
        public int unk0;
        public int unk1;
        public int unk2;
        public int unk3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public long[] unkPtrArray; // Has 17 pointers and I have no idea what they are
        public int unk4; // 2?
        public float stamina;
        public long unkPtr1;
        public int unk5;
        public int unk6;
        public int unk7;
        public int unk8;
        public float maxPossibleStamina;
        public int unk9;
        public float maxStamina;
        public int selectedItemId;
        public long unkPtr2;
        public float CurrentExtTimer;
        public float MaxExtTimer;
        public byte unk10;
        public byte unk11;
        public byte unk12;
        public byte unk13;
        public int unk14;
        public int isExtGaugeVisible; // bool
    }
}
