using System;
using System.Collections.Generic;
using System.IO;
using HunterPie.Logger;

namespace HunterPie.Memory {
    class Address {
        class Offsets {
            public static Int64[] LevelOffsets = new Int64[4] { 0x70, 0x68, 0x8, 0x20 };
            public static Int64 LevelLastOffset = 0x108;

            public static Int64[] ZoneOffsets = new Int64[4] { 0x660, 0x28, 0x18, 0x440 };
            public static Int64 ZoneLastOffset = 0x2B0;

            public static Int64[] WeaponOffsets = new Int64[4] { 0x70, 0x5A8, 0x310, 0x148 };
            public static Int64 WeaponLastOffset = 0x2B8;

            public static Int64[] SessionOffsets = new Int64[4] { 0xA0, 0x20, 0x80, 0x9C };
            public static Int64 SessionLastOffset = 0x3C8;

            public static Int64[] EquipmentOffsets = new Int64[4] { 0x78, 0x50, 0x40, 0x450 };
            public static Int64 EquipmentLastOffset = 0x0;

            public static Int64[] PartyOffsets = new Int64[1] { 0x0 };
            public static Int64 PartyLastOffset = 0x0;

            public static Int64[] MonsterOffsets = new Int64[2] { 0xAF738, 0x47CDE0 };
            public static Int64 MonsterLastOffset = 0x0;
        }
        public static int GAME_VERSION = 168031;

        // Static addresses
        public static Int64 BASE = 0x140000000;
        public static Int64 LEVEL_OFFSET = 0x3B4C2B8;
        public static Int64 ZONE_OFFSET = 0x48F2E60;
        public static Int64 MONSTER_OFFSET = 0x48E06E8;
        public static Int64 SESSION_OFFSET = 0x48E85C0;
        public static Int64 EQUIPMENT_OFFSET = 0x3B50668;
        public static Int64 WEAPON_OFFSET = 0x3BEDE58;
        public static Int64 PARTY_OFFSET = 0x48E1850;

        // Consts
        public const Int64 cooldownFixed = 0x9EC;
        public const Int64 cooldownDynamic = 0x99C;
        public const Int64 timerFixed = 0xADC;
        public const Int64 timerDynamic = 0xA8C;

        // Loaded values
        private static Dictionary<string, Int64> MappedAddresses = new Dictionary<string, Int64>();
        
        public static bool LoadMemoryMap(int version) {
            string FILE_NAME = $"MonsterHunterWorld.{version}.map";
            // If dir or file doesn't exist
            if (!Directory.Exists("address")) return false;
            if (!File.Exists($"address/{FILE_NAME}")) return false;
            // Load file
            if (!isOlderThanIceborne(version)) {
                // Update offsets for iceborne
                UpdateToIceborneOffsets();
            }
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

        // Support Iceborne and older versions
        private static bool isOlderThanIceborne(int game_version) {
            return game_version <= GAME_VERSION;
        }

        private static void UpdateToIceborneOffsets() {
            Offsets.LevelOffsets = new Int64[4] { 0x70, 0x18, 0x18, 0xE8 };
            Offsets.LevelLastOffset = 0x90;

            Offsets.ZoneOffsets = new Int64[4] { 0xB8, 0x58, 0x18, 0x6D0 };
            Offsets.ZoneLastOffset = 0x358;

            Offsets.SessionOffsets = new Int64[4] { 0x1C0, 0x1B8, 0x8, 0x30 };
            Offsets.SessionLastOffset = 0x3C8;
            Debugger.Warn("Updated offsets to Iceborne's version");
        }
    }
}
