using System.Runtime.InteropServices;

/*
    Category explanation:
    - When Category is 3 and Type is uint.MaxValue, then the Id is for a mantle
    - When Category is 2 and Type is 5, then the Id is for a charm.
    - When Category is 1, Type can go from 0 to 13 and determines a class weapon (See the Classes enum for the corresponding Type)
    - When Category is 0 or 2 and Type goes from 0 to 4, then it's an armor.
    - When category is uint.MaxValue then the player does not have that gear.
*/

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential, Size = 0xA0)]
    public struct sGear
    {
        long address;
        public int AbsoluteSlot; // Moving the item to another slot doesn't change this
        public int Slot;  // Moving the ithem to another slot DOES change this
        long unk1;
        public uint Category; // Gear category, read the explanation above
        public int unk3;
        public uint Type; // Type depends on category
        public int Id;
        public int Level;
        public int ExperienceRemaining; // Upgrade points remaining to next level
        public uint DecorationSlot1;
        public uint DecorationSlot2;
        public uint DecorationSlot3;
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
