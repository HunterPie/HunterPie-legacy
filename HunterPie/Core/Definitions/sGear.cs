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
        public int HasItem; // 1 when you dont have the gear, 2 when you do have the gear
        public int unk3;
        public int Type;
        public int Id;
        public int Level;
        public int ExperienceRemaining; // Upgrade points remaining to next level
        public uint unk8;
        public uint unk9;
        public uint unk10;
        public uint unk11;
        public uint unk12;
        public uint unk13;
        public uint unk14;
        public uint unk15;
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
