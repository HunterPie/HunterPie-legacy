using System;
using System.IO;

namespace HunterPie.Memory {
    class Address {

        public static string GAME_VERSION = "167907";

        // Static addresses
        public static Int64 BASE = 0x140000000;
        public static Int64 LEVEL_OFFSET = 0x03B4A278;
        public static Int64 ZONE_OFFSET = 0x048F0E20;
        public static Int64 MONSTER_OFFSET = 0x48DE698;
        public static Int64 SESSION_OFFSET = 0x048D95E0;
        public static Int64 EQUIPMENT_OFFSET = 0x03BE71E0;
        public static Int64 WEAPON_OFFSET = 0x03BEBE18;
        public static Int64 PARTY_OFFSET = 0x48DF800;

        // Consts
        public const Int64 cooldownFixed = 0x9EC;
        public const Int64 cooldownDynamic = 0x99C;
        public const Int64 timerFixed = 0xADC;
        public const Int64 timerDynamic = 0xA8C;

        public static bool LoadOlderGameVersion(string version) {
            string FILE_NAME = $"MonsterHunterWorld.{version}.map";
            // If dir or file doesn't exist
            if (!Directory.Exists("address")) return false;
            if (!File.Exists($"address/{FILE_NAME}")) return false;
            // Load file
            LoadMemoryAddresses(FILE_NAME);
            GAME_VERSION = version;
            return true;
        }

        private static void LoadMemoryAddresses(string filename) {
            string[] fileLines = File.ReadAllLines($"address/{filename}");
            foreach (string line in fileLines) {
                string[] parsed = line.Split(' ');
                // parsed[0] = type
                // parsed[1] = name
                // parsed[2] = value
                if (parsed[0] == "long") {
                    switch(parsed[1]) {
                        case "BASE":
                            BASE = ParseHex(parsed[2]);
                            break;
                        case "LEVEL_OFFSET":
                            LEVEL_OFFSET = ParseHex(parsed[2]);
                            break;
                        case "ZONE_OFFSET":
                            ZONE_OFFSET = ParseHex(parsed[2]);
                            break;
                        case "MONSTER_OFFSET":
                            MONSTER_OFFSET = ParseHex(parsed[2]);
                            break;
                        case "SESSION_OFFSET":
                            SESSION_OFFSET = ParseHex(parsed[2]);
                            break;
                        case "EQUIPMENT_OFFSET":
                            EQUIPMENT_OFFSET = ParseHex(parsed[2]);
                            break;
                        case "WEAPON_OFFSET":
                            WEAPON_OFFSET = ParseHex(parsed[2]);
                            break;
                        case "PARTY_OFFSET":
                            PARTY_OFFSET = ParseHex(parsed[2]);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private static Int64 ParseHex(string hexstring) {
            Int64 result = Convert.ToInt64(hexstring.Replace("0x", ""), 16);
            return result;
        }
    }
}
