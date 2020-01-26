using System.Collections.Generic;

namespace HunterPie.Core {
    class GStrings {

        static private Dictionary<int, string> Zones = new Dictionary<int, string>();
        static private Dictionary<int, string> Weapons = new Dictionary<int, string>();
        static private Dictionary<string, string> Monsters = new Dictionary<string, string>();
        static private Dictionary<int, string> Fertilizers = new Dictionary<int, string>();
        static private Dictionary<int, string> Mantles = new Dictionary<int, string>();

        public static void InitStrings() {
            PopulateMonsters();
            PopulateWeapons();
            PopulateZones();
            PopulateFertilizers();
            PopulateMantles();
        }

        private static void PopulateMantles() {
            Mantles.Add(0, "Ghillie Mantle");
            Mantles.Add(1, "Temporal Mantle");
            Mantles.Add(2, "Health Booster");
            Mantles.Add(3, "Rocksteady Mantle");
            Mantles.Add(4, "Challenger Mantle");
            Mantles.Add(5, "Vitality Mantle");
            Mantles.Add(6, "Fireproof Mantle");
            Mantles.Add(7, "Waterproof Mantle");
            Mantles.Add(8, "Iceproof Mantle");
            Mantles.Add(9, "Thunderproof Mantle");
            Mantles.Add(10, "Dragonproof Mantle");
            Mantles.Add(11, "Cleanser Booster");
            Mantles.Add(12, "Glider Mantle");
            Mantles.Add(13, "Evasion Mantle");
            Mantles.Add(14, "Impact Mantle");
            Mantles.Add(15, "Apothecary Mantle");
            Mantles.Add(16, "Immunity Mantle");
            Mantles.Add(17, "Affinity Booster");
            Mantles.Add(18, "Bandit Mantle");
            Mantles.Add(19, "Assassin's Hood");
        }

        private static void PopulateFertilizers() {
            Fertilizers.Add(0, "Empty");
            Fertilizers.Add(1, "Plant Harvest Up (S)");
            Fertilizers.Add(2, "Plant Harvest Up (L)");
            Fertilizers.Add(3, "Fungi Harvest Up (S)");
            Fertilizers.Add(4, "Fungi Harvest Up (L)");
            Fertilizers.Add(5, "Bug/Honey Harvest Up (S)");
            Fertilizers.Add(6, "Bug/Honey Harvest Up (L)");
            Fertilizers.Add(7, "Growth Up (S)");
            Fertilizers.Add(8, "Growth Up (L)");
        }

        private static void PopulateZones() {
            Zones.Add(0, "Main Menu");
            Zones.Add(101, "Ancient Forest");
            Zones.Add(102, "Wildspire Waste");
            Zones.Add(103, "Coral Highlands");
            Zones.Add(104, "Rotten Vale");
            Zones.Add(105, "Elder's Recess");
            Zones.Add(106, "Great Ravine");
            Zones.Add(107, "Great Ravine");
            Zones.Add(108, "Hoarfrost Reach");
            Zones.Add(109, "The Guiding Lands");
            Zones.Add(201, "Special Arena");
            Zones.Add(202, "Arena");
            Zones.Add(301, "Astera");
            Zones.Add(302, "Gathering Hub");
            Zones.Add(303, "Research Base");
            Zones.Add(305, "Seliana");
            Zones.Add(306, "Gathering Hub"); // Seliana's
            Zones.Add(401, "Ancient Forest"); // Intro
            Zones.Add(403, "Everstream");
            Zones.Add(405, "Confluence of Fates");
            Zones.Add(406, "Ancient Forest"); // Tutorial
            Zones.Add(409, "Caverns of El Dorado");
            Zones.Add(501, "Living Quarters");
            Zones.Add(502, "Private Quarters");
            Zones.Add(503, "Private Suite");
            Zones.Add(504, "Training Area");
            Zones.Add(505, "Chamber of Five");
            Zones.Add(506, "Seliana Room");
        }

        private static void PopulateWeapons() {
            Weapons.Add(0, "Greatsword");
            Weapons.Add(1, "Sword and Shield");
            Weapons.Add(2, "Dual Blades");
            Weapons.Add(3, "Long Sword");
            Weapons.Add(4, "Hammer");
            Weapons.Add(5, "Hunting Horn");
            Weapons.Add(6, "Lance");
            Weapons.Add(7, "Gunlance");
            Weapons.Add(8, "Switch Axe");
            Weapons.Add(9, "Charge Blade");
            Weapons.Add(10, "Insect Glaive");
            Weapons.Add(11, "Bow");
            Weapons.Add(12, "Heavy Bowgun");
            Weapons.Add(13, "Light Bowgun");
        }

        private static void PopulateMonsters() {
            Monsters.Add("em001_00", "Rathian");
            Monsters.Add("em001_01", "Pink Rathian");
            Monsters.Add("em001_02", "Gold Rathian");
            Monsters.Add("em002_00", "Rathalos");
            Monsters.Add("em002_01", "Azure Rathalos");
            Monsters.Add("em002_02", "Silver Rathalos");
            Monsters.Add("em007_00", "Diablos");
            Monsters.Add("em007_01", "Black Diablos");
            Monsters.Add("em011_00", "Kirin");
            Monsters.Add("em018_00", "Yian Garuga");
            Monsters.Add("em018_05", "Scarred Yian Garuga");
            Monsters.Add("em024_00", "Kushala Daora");
            Monsters.Add("em026_00", "Lunastra");
            Monsters.Add("em027_00", "Teostra");
            Monsters.Add("em032_00", "Tigrex");
            Monsters.Add("em032_01", "Brute Tigrex");
            Monsters.Add("em036_00", "Lavasioth");
            Monsters.Add("em037_00", "Nargacuga");
            Monsters.Add("em042_00", "Barioth");
            Monsters.Add("em043_00", "Deviljho");
            Monsters.Add("em043_05", "Savage Deviljho");
            Monsters.Add("em044_00", "Barroth");
            Monsters.Add("em045_00", "Uragaan");
            Monsters.Add("em057_00", "Zinogre");
            Monsters.Add("em063_00", "Brachydios");
            Monsters.Add("em080_00", "Glavenus");
            Monsters.Add("em080_01", "Acidic Glavenus");
            Monsters.Add("em100_00", "Anjanath");
            Monsters.Add("em100_01", "Fulgur Anjanath");
            Monsters.Add("em101_00", "Great Jagras");
            Monsters.Add("em102_00", "Pukei-Pukei");
            Monsters.Add("em102_01", "Coral Pukei-Pukei");
            Monsters.Add("em103_00", "Nergigante");
            Monsters.Add("em103_05", "Ruiner Nergigante");
            Monsters.Add("em105_00", "Xeno'jiiva");
            Monsters.Add("em106_00", "Zorah Magdaros");
            Monsters.Add("em107_00", "Kulu-Ya-Ku");
            Monsters.Add("em108_00", "Jyuratodus");
            Monsters.Add("em109_00", "Tobi-Kadachi");
            Monsters.Add("em109_01", "Viper Tobi-Kadachi");
            Monsters.Add("em110_00", "Paolumu");
            Monsters.Add("em110_01", "Nightshade Paolumu");
            Monsters.Add("em111_00", "Legiana");
            Monsters.Add("em111_05", "Shrieking Legiana");
            Monsters.Add("em112_00", "Great Girros");
            Monsters.Add("em113_00", "Odogaron");
            Monsters.Add("em113_01", "Ebony Odogaron");
            Monsters.Add("em114_00", "Radobaan");
            Monsters.Add("em115_00", "Vaal Hazak");
            Monsters.Add("em115_05", "Blackveil Vaal Hazak");
            Monsters.Add("em116_00", "Dodogama");
            Monsters.Add("em117_00", "Kulve Taroth");
            Monsters.Add("em118_00", "Bazelgeuse");
            Monsters.Add("em118_05", "Seething Bazelgeuse");
            Monsters.Add("em120_00", "Tzitzi-Ya-Ku");
            Monsters.Add("em121_00", "Behemoth");
            Monsters.Add("em122_00", "Beotodus");
            Monsters.Add("em123_00", "Banbaro");
            Monsters.Add("em124_00", "Velkhana");
            Monsters.Add("em125_00", "Namielle");
            Monsters.Add("em126_00", "Shara Ishvalda");
            Monsters.Add("em127_00", "Leshen");
            Monsters.Add("em127_01", "Ancient Leshen");
        }

        public static string MantleName(int ID) {
            if (Mantles.ContainsKey(ID)) {
                return Mantles[ID];
            } else {
                return null;
            }
        }

        public static string WeaponName(int ID) {
            if (Weapons.ContainsKey(ID)) {
                return Weapons[ID];
            } else {
                return null;
            }
        }

        public static string MonsterName(string ID) {
            if (Monsters.ContainsKey(ID)) {
                return Monsters[ID];
            } else {
                return null;
            }
        }

        public static string FertilizerName(int ID) {
            if (Fertilizers.ContainsKey(ID)) {
                return Fertilizers[ID];
            } else {
                return null;
            }
        }

        public static string ZoneName(int ID) {
            if (Zones.ContainsKey(ID)) {
                return Zones[ID];
            } else {
                return null;
            }
        }

    }
}
