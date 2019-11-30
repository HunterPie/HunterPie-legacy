using System;
using System.Collections.Generic;
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

        // Loaded values
        private static Dictionary<string, Int64> MappedAddresses = new Dictionary<string, Int64>();
        
        public static bool LoadMemoryMap(string version) {
            string FILE_NAME = $"MonsterHunterWorld.{version}.map";
            // If dir or file doesn't exist
            if (!Directory.Exists("address")) return false;
            if (!File.Exists($"address/{FILE_NAME}")) return false;
            // Load file
            LoadMemoryAddresses(FILE_NAME);
            GAME_VERSION = version;
            LoadValuesToMemory();
            return true;
        }

        private static void LoadMemoryAddresses(string filename) {
            // Clear all loaded values
            MappedAddresses.Clear();
            string[] fileLines = File.ReadAllLines($"address/{filename}");
            foreach (string line in fileLines) {
                string[] parsed = line.Split(' ');
                // parsed[0] = type
                // parsed[1] = name
                // parsed[2] = value
                Int64 parsedValue;
                try {
                    parsedValue = ParseHex(parsed[2]);
                } catch {
                    Debugger.Error($"Failed parsing value for \"{parsed[1]}\"");
                    parsedValue = 0xFFFFFFFF;
                }
                MappedAddresses.Add(parsed[1], parsedValue);
            }
        }

        private static Int64 ParseHex(string hexstring) {
            Int64 result = Convert.ToInt64(hexstring.Replace("0x", ""), 16);
            return result;
        }

        private static void LoadValuesToMemory() {
            // kill me pls
            try {
                BASE = MappedAddresses["BASE"] == 0xFFFFFFFF ? BASE : MappedAddresses["BASE"];
            } catch {
                Debugger.Error($"MonsterHunterWorld.{GAME_VERSION}.map missing value for BASE");
            }
            try {
                LEVEL_OFFSET = MappedAddresses["LEVEL_OFFSET"] == 0xFFFFFFFF ? LEVEL_OFFSET : MappedAddresses["LEVEL_OFFSET"];
            } catch {
                Debugger.Error($"MonsterHunterWorld.{GAME_VERSION}.map missing value for LEVEL_OFFSET");
            }
            try {
                ZONE_OFFSET = MappedAddresses["ZONE_OFFSET"] == 0xFFFFFFFF ? ZONE_OFFSET : MappedAddresses["ZONE_OFFSET"];
            } catch {
                Debugger.Error($"MonsterHunterWorld.{GAME_VERSION}.map missing value for ZONE_OFFSET");
            }
            try {
                MONSTER_OFFSET = MappedAddresses["MONSTER_OFFSET"] == 0xFFFFFFFF ? MONSTER_OFFSET : MappedAddresses["MONSTER_OFFSET"];
            } catch {
                Debugger.Error($"MonsterHunterWorld.{GAME_VERSION}.map missing value for MONSTER_OFFSET");
            }
            try {
                SESSION_OFFSET = MappedAddresses["SESSION_OFFSET"] == 0xFFFFFFFF ? SESSION_OFFSET : MappedAddresses["SESSION_OFFSET"];
            } catch {
                Debugger.Error($"MonsterHunterWorld.{GAME_VERSION}.map missing value for SESSION_OFFSET");
            }
            try {
                EQUIPMENT_OFFSET = MappedAddresses["EQUIPMENT_OFFSET"] == 0xFFFFFFFF ? EQUIPMENT_OFFSET : MappedAddresses["EQUIPMENT_OFFSET"];
            } catch {
                Debugger.Error($"MonsterHunterWorld.{GAME_VERSION}.map missing value for EQUIPMENT_OFFSET");
            }
            try {
                WEAPON_OFFSET = MappedAddresses["WEAPON_OFFSET"] == 0xFFFFFFFF ? WEAPON_OFFSET : MappedAddresses["WEAPON_OFFSET"];
            } catch {
                Debugger.Error($"MonsterHunterWorld.{GAME_VERSION}.map missing value for WEAPON_OFFSET");
            }
            try {
                PARTY_OFFSET = MappedAddresses["PARTY_OFFSET"] == 0xFFFFFFFF ? PARTY_OFFSET : MappedAddresses["PARTY_OFFSET"];
            } catch {
                Debugger.Error($"MonsterHunterWorld.{GAME_VERSION}.map missing value for PARTY_OFFSET");
            }
        }
    }
}
