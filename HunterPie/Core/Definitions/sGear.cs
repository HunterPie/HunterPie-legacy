using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential, Size = 0xA0)]
    public struct sGear
    {
        long address;
        public int AbsoluteSlot; // Moving the item to another slot doesn't change this
        public int Slot;  // Moving the ithem to another slot DOES change this
        long unk1;
        int unk2;
        int unk3;
        public int Type;
        public int Id;
        public int Level;
        public int ExperienceRemaining; // Upgrade points remaining to next level
        uint unk8;
        uint unk9;
        uint unk10;
        uint unk11;
        uint unk12;
        uint unk13;
        uint unk14;
        uint unk15;
        public int AugmentId; // First augment
        int unk17;
        int unk18;
        uint unk19;
        uint unk20;
        uint unk21;
        int unk22;
        int unk23;
        int unk24;
        int unk25;
        uint unk26;
        uint unk27;
        uint unk28;
        uint unk29;
        uint unk30;
        int unk31;
        int unk32;
        int unk33;
    };
}
