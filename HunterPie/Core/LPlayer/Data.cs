namespace HunterPie.Core.LPlayer {
    /*
     Player data structs, they'll be used in the future for some stuff I want to implement :)
    */
    public class GameStructs {
        public struct Weapon {
            public int Type;
            public int ID;
            public int[] Decorations;
            public int[] Augments;
        }
        public struct Armor {
            public int ID;
            public int[] Decorations;
            public int[] Augments;
        }
        public struct Charm {
            public int ID;
        }
        public struct Mantle {
            public int ID;
            public int[] Decorations;
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

    }
}
