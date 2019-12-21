using System;
using System.Collections.Generic;
using System.IO;
using HunterPie.Logger;

namespace HunterPie.Memory {
    class Address {

        public static string GAME_VERSION = "167907";

        // Static addresses
        public static Int64 BASE = 0x140000000;
        public static Int64 LEVEL_OFFSET = 0x03B4A278;
        public static Int64 ZONE_OFFSET = 0x048F0E20;
        public static Int64 MONSTER_OFFSET = 0x48DE698;
        public static Int64 SESSION_OFFSET = 0x048E6570;
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
                // parsed[0]: type
                // parsed[1]: name
                // parsed[2]: value
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
            LoadFromDict(nameof(BASE), out BASE, BASE);
            LoadFromDict(nameof(LEVEL_OFFSET), out LEVEL_OFFSET, LEVEL_OFFSET);
            LoadFromDict(nameof(ZONE_OFFSET), out ZONE_OFFSET, ZONE_OFFSET);
            LoadFromDict(nameof(MONSTER_OFFSET), out MONSTER_OFFSET, MONSTER_OFFSET);
            LoadFromDict(nameof(SESSION_OFFSET), out SESSION_OFFSET, SESSION_OFFSET);
            LoadFromDict(nameof(EQUIPMENT_OFFSET), out EQUIPMENT_OFFSET, EQUIPMENT_OFFSET);
            LoadFromDict(nameof(WEAPON_OFFSET), out WEAPON_OFFSET, WEAPON_OFFSET);
            LoadFromDict(nameof(PARTY_OFFSET), out PARTY_OFFSET, PARTY_OFFSET);
        }

        private static void LoadFromDict(string name, out Int64 variable, Int64 oldValue) {
            try {
                variable = MappedAddresses[name] == 0xFFFFFFFF ? oldValue : MappedAddresses[name];
            } catch {
                variable = oldValue;
                Debugger.Error($"MonsterHunterWorld.{GAME_VERSION}.map missing value for {name}");
            }
        }
    }
}
