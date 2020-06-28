namespace HunterPie.Core.LPlayer
{
    /*
     Player data structs, they'll be used in the future for some stuff I want to implement :)
    */
    public class GameStructs
    {
        public struct Decoration
        {
            public int ID;
        }

        public struct Augment
        {
            public int ID;
        }

        public struct Weapon
        {
            public int Type;
            public int ID;
            public Decoration[] Decorations;
            public Augment[] Augments;
            public NewAugment[] NewAugments;
            public AwakenedSkill[] Awakenings;
            public CustomAugment[] CustomAugments;
            public BowgunMod[] BowgunMods;
        }

        public struct Armor
        {
            public int ID;
            public Decoration[] Decorations;
        }

        public struct Charm
        {
            public int ID;
        }

        public struct SpecializedTool
        {
            public int ID;
            public Decoration[] Decorations;
        }

        public struct NewAugment
        {
            public byte ID;
            public byte Level;
        }

        public struct CustomAugment
        {
            public byte ID;
            public byte Level;
        }

        public struct AwakenedSkill
        {
            public short ID;
        }

        public struct BowgunMod
        {
            public int ID;
        }

        public struct Gear
        {
            public Weapon Weapon;
            public Armor Helmet;
            public Armor Chest;
            public Armor Hands;
            public Armor Waist;
            public Armor Legs;
            public Charm Charm;
            public SpecializedTool[] SpecializedTools;
        }

        public static int ConvertToMax(uint ID) => ID < 0xFFFFFFFF ? (int)ID : int.MaxValue;
    }
}
