namespace HunterPie.Core.LPlayer {
    /*
     Player data structs, they'll be used in the future for some stuff I want to implement :)
    */
    public class GameStructs {
        public struct Decoration {
            public int ID;
        }
        public struct Augment {
            public int ID;
        }
        public struct Weapon {
            public int Type;
            public int ID;
            public Decoration[] Decorations;
            public Augment[] Augments;
        }
        public struct Armor {
            public int ID;
            public Decoration[] Decorations;
        }
        public struct Charm {
            public int ID;
        }
        public struct Mantle {
            public int ID;
            public Decoration[] Decorations;
        }
        public struct Gear {
            public Weapon Weapon;
            public Armor Helmet;
            public Armor Chest;
            public Armor Hands;
            public Armor Waist;
            public Armor Legs;
            public Charm Charm;
            public Mantle[] Mantles;
        }
        public static int ConvertToZero(uint ID) {
            return ID < 0xFFFFFFFF ? (int)ID : 0;
        }
    }
}
