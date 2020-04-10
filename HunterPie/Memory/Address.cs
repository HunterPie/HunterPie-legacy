using System;
using System.Collections.Generic;
using System.IO;
using HunterPie.Logger;

namespace HunterPie.Memory {
    class Address {
        public class Offsets {
            public static int[] LevelOffsets;
            public static int[] ZoneOffsets;
            public static int[] WeaponOffsets;
            public static int[] SessionOffsets;
            public static int[] EquipmentOffsets;
            public static int[] PartyOffsets;
            public static int[] DamageOffsets;
            public static int[] AbnormalityOffsets;
            public static int[] MonsterOffsets;
            public static int[] MonsterSelectedOffsets;
            public static int[] PlayerGearOffsets;
            

            public static int FertilizersOffset;
            public static int TailRaidersOffset = 0x10344C;
            public static int SteamFuelOffset = 0x102F4C;
            public static int ArgosyOffset = 0x103430;
            public static int HarvestBoxOffset = 0x10;

            // Monster
            public static int MonsterHPComponentOffset;
            public static int MonsterNamePtr;
            public static int MonsterPartsOffset = 0x14528;
            public static int FirstMonsterPartOffset = 0x1C;
            public static int NextMonsterPartOffset = 0x1F8;
            public static int MonsterPartBrokenCounterOffset = 0xC;
            public static int RemovablePartsOffset = 0x164C0;
            public static int NextRemovablePart = 0x78;
            public static int[] MonsterAilmentsOffsets = new int[2] { 0x4C0, 0x5E60 };

        }
        public static int PREICEBORNE_VERSION = 168031;
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
        public static Int64 DAMAGE_OFFSET = 0x0;
        public static Int64 ABNORMALITY_OFFSET = 0x0;
        public static Int64 MONSTER_SELECTED_OFFSET = 0x0;
        public static Int64 MONSTER_TARGETED_OFFSET = 0x0;

        // Consts
        public const Int64 cooldownFixed = 0x9EC;
        public const Int64 cooldownDynamic = 0x99C;
        public const Int64 timerFixed = 0xADC;
        public const Int64 timerDynamic = 0xA8C;

        // Loaded values
        private static Dictionary<string, Int64> MappedAddresses = new Dictionary<string, Int64>();
        private static Dictionary<string, int[]> MappedOffsets = new Dictionary<string, int[]>();
        
        public static bool LoadMemoryMap(int version) {
            string FILE_NAME = $"MonsterHunterWorld.{version}.map";
            // If dir or file doesn't exist
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "address"))) return false;
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"address/{FILE_NAME}"))) return false;
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
            MappedOffsets.Clear();
            string[] fileLines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"address/{filename}"));
            foreach (string line in fileLines) {
                if (line.StartsWith("#")) continue; // Ignore comments
                string[] parsed = line.Split(' ');
                // parsed[0]: type
                // parsed[1]: name
                // parsed[2]: value
                AddValueToMap(parsed[0], parsed);
            }
        }

        private static void AddValueToMap(string type, string[] values) {
            string name = values[1];
            string value = values[2];
            switch(type) {
                case "Address":
                case "long":
                    Int64 parsedValue;
                    try {
                        parsedValue = ParseHex(value);
                    } catch {
                        Debugger.Error($"Failed parsing value for \"{name}\"");
                        parsedValue = 0xFFFFFFFF;
                    }
                    MappedAddresses.Add(name, parsedValue);
                    break;
                case "Offset":
                    string[] strOffsets = value.Split(',');
                    int[] offsets = new int[strOffsets.Length];
                    for (int i = 0; i < strOffsets.Length; i++) {
                        try {
                            offsets[i] = ParseHexToInt32(strOffsets[i]);
                        } catch {
                            Debugger.Error($"Failed to parse value {strOffsets[i]}");
                            offsets[i] = 0xFF;
                        }
                    }
                    MappedOffsets.Add(name, offsets);
                    break;
                default:
                    Debugger.Error($"Invalid type: {type}");
                    break;
            }
        }

        private static int ParseHexToInt32(string hexstring) {
            return Convert.ToInt32(hexstring.Replace("0x", ""), 16);
        }

        private static Int64 ParseHex(string hexstring) {
            return Convert.ToInt64(hexstring.Replace("0x", ""), 16);
        }

        private static void LoadValuesToMemory() {
            LoadAddressFromDict(nameof(BASE), out BASE, BASE);
            LoadAddressFromDict(nameof(LEVEL_OFFSET), out LEVEL_OFFSET, LEVEL_OFFSET);
            LoadAddressFromDict(nameof(ZONE_OFFSET), out ZONE_OFFSET, ZONE_OFFSET);
            LoadAddressFromDict(nameof(MONSTER_OFFSET), out MONSTER_OFFSET, MONSTER_OFFSET);
            LoadAddressFromDict(nameof(EQUIPMENT_OFFSET), out EQUIPMENT_OFFSET, EQUIPMENT_OFFSET);
            LoadAddressFromDict(nameof(WEAPON_OFFSET), out WEAPON_OFFSET, WEAPON_OFFSET);
            LoadAddressFromDict(nameof(SESSION_OFFSET), out SESSION_OFFSET, SESSION_OFFSET);
            LoadAddressFromDict(nameof(PARTY_OFFSET), out PARTY_OFFSET, PARTY_OFFSET);
            LoadAddressFromDict(nameof(DAMAGE_OFFSET), out DAMAGE_OFFSET, DAMAGE_OFFSET);
            LoadAddressFromDict(nameof(ABNORMALITY_OFFSET), out ABNORMALITY_OFFSET, ABNORMALITY_OFFSET);
            LoadAddressFromDict(nameof(MONSTER_SELECTED_OFFSET), out MONSTER_SELECTED_OFFSET, MONSTER_SELECTED_OFFSET);
            LoadAddressFromDict(nameof(MONSTER_TARGETED_OFFSET), out MONSTER_TARGETED_OFFSET, MONSTER_TARGETED_OFFSET);
            // Load offsets
            LoadOffsetsFromDict("LevelOffsets", out Offsets.LevelOffsets, Offsets.LevelOffsets);
            LoadOffsetsFromDict("ZoneOffsets", out Offsets.ZoneOffsets, Offsets.ZoneOffsets);
            LoadOffsetsFromDict("SessionOffsets", out Offsets.SessionOffsets, Offsets.SessionOffsets);
            LoadOffsetsFromDict("WeaponOffsets", out Offsets.WeaponOffsets, Offsets.WeaponOffsets);
            LoadOffsetsFromDict("MonsterOffsets", out Offsets.MonsterOffsets, Offsets.MonsterOffsets);
            LoadOffsetsFromDict("EquipmentOffsets", out Offsets.EquipmentOffsets, Offsets.EquipmentOffsets);
            LoadOffsetsFromDict("PartyOffsets", out Offsets.PartyOffsets, Offsets.PartyOffsets);
            LoadOffsetsFromDict("DamageOffsets", out Offsets.DamageOffsets, Offsets.DamageOffsets);
            LoadOffsetsFromDict("AbnormalityOffsets", out Offsets.AbnormalityOffsets, Offsets.AbnormalityOffsets);
            LoadOffsetsFromDict("MonsterSelectedOffsets", out Offsets.MonsterSelectedOffsets, Offsets.MonsterSelectedOffsets);
            LoadOffsetsFromDict("PlayerGearOffsets", out Offsets.PlayerGearOffsets, Offsets.PlayerGearOffsets);
            // Clear addresses loaded into memory
            MappedAddresses.Clear();
            MappedOffsets.Clear();
        }

        private static void LoadAddressFromDict(string name, out Int64 variable, Int64 oldValue) {
            try {
                variable = MappedAddresses[name] == 0xFFFFFFFF ? oldValue : MappedAddresses[name];
            } catch {
                variable = oldValue;
                Debugger.Error($"MonsterHunterWorld.{GAME_VERSION}.map missing value for {name}");
            }
        }

        private static void LoadOffsetsFromDict(string name, out int[] offsetsArray, int[] oldOffsetsArray) {
            try {
                offsetsArray = MappedOffsets[name];
            } catch {
                offsetsArray = oldOffsetsArray;
                Debugger.Error($"MonsterHunterWorld.{GAME_VERSION}.map missing offsets for {name}");
            }
        }

        // Support Iceborne and older versions
        private static bool isOlderThanIceborne(int game_version) {
            if (game_version <= PREICEBORNE_VERSION) {
                Debugger.Error("Pre-Iceborne game not supported anymore.");
            }
            return game_version <= PREICEBORNE_VERSION;
        }

        private static void UpdateToIceborneOffsets() {
            Offsets.FertilizersOffset = 0x102FE4;
            Offsets.HarvestBoxOffset = 0x20;

            // Monster Iceborne offsets
            
            Offsets.MonsterHPComponentOffset = 0x7670;
            Offsets.MonsterNamePtr = 0x2a0;
        }
    }
}
