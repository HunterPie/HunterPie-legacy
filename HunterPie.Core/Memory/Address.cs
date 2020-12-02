using System;
using System.Collections.Generic;
using System.IO;
using HunterPie.Logger;

namespace HunterPie.Memory
{
    // TODO: Refactor this code and use dictionary instead of variables
    public class Address
    {
        public class Offsets
        {
            public static int[] LevelOffsets;
            public static int[] ZoneOffsets;
            public static int[] WeaponOffsets;
            public static int[] SessionOffsets;
            public static int[] EquipmentOffsets;
            public static int[] PartyOffsets;
            public static int[] DamageOffsets;
            public static int[] AbnormalityOffsets;
            public static int[] SkillOffsets;
            public static int[] AbnormalityGearOffsets;
            public static int[] MonsterOffsets;
            public static int[] MonsterSelectedOffsets;
            public static int[] PlayerGearOffsets;
            public static int[] PlayerLockonOffsets;
            public static int[] WeaponMechanicsOffsets;
            public static int[] PlayerPositionOffsets;
            public static int[] PlayerBasicInformationOffsets;
            public static int[] WeaponDataOffsets;
            public static int[] gHudStaminaBarOffsets;
            public static int[] gHudHealthBarOffsets;

            public static readonly int FertilizersOffset = 0x102FE4;
            public static readonly int TailRaidersOffset = 0x10344C;
            public static readonly int SteamFuelOffset = 0x102F4C;
            public static readonly int ArgosyOffset = 0x103430;
            public static readonly int HarvestBoxOffset = 0x20;
            public static int[] PlayerCanteenTimer = new int[1] { 0x0 };
            // Monster
            public static readonly int MonsterHPComponentOffset = 0x7670;
            public static readonly int MonsterNamePtr = 0x2A0;
            public static readonly int MonsterGameIDOffset = 0x12280;
            public static readonly int MonsterPartsOffset = 0x14528;
            public static readonly int FirstMonsterPartOffset = 0x1C;
            public static readonly int NextMonsterPartOffset = 0x1F8;
            public static readonly int RemovablePartsOffset = 0x164C0;
            public static readonly int NextRemovablePart = 0x78;
            public static int[] MonsterAilmentsOffsets = new int[2] { 0x4C0, 0x5E60 };

        }
        public static int PREICEBORNE_VERSION = 168031;
        public static int GAME_VERSION = 168031;

        // Static addresses
        public static long BASE;
        public static long LEVEL_OFFSET;
        public static long ZONE_OFFSET;
        public static long MONSTER_OFFSET;
        public static long SESSION_OFFSET;
        public static long EQUIPMENT_OFFSET;
        public static long WEAPON_OFFSET;
        public static long PARTY_OFFSET;
        public static long DAMAGE_OFFSET;
        public static long ABNORMALITY_OFFSET;
        public static long MONSTER_SELECTED_OFFSET;
        public static long MONSTER_TARGETED_OFFSET;
        public static long WEAPON_MECHANICS_OFFSET;
        public static long CANTEEN_OFFSET;
        public static long WORLD_DATA_OFFSET;
        public static long WEAPON_DATA_OFFSET;
        public static long HUD_DATA_OFFSET;

        // Consts
        public const long CooldownFixed = 0x9EC;
        public const long CooldownDynamic = 0x99C;
        public const long TimerFixed = 0xADC;
        public const long TimerDynamic = 0xA8C;

        // Loaded values
        private static readonly Dictionary<string, long> mappedAddresses = new Dictionary<string, long>();
        private static readonly Dictionary<string, int[]> mappedOffsets = new Dictionary<string, int[]>();

        public static bool LoadMemoryMap(int version)
        {
            string FILE_NAME = $"MonsterHunterWorld.{version}.map";
            // If dir or file doesn't exist
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "address"))) return false;
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"address/{FILE_NAME}"))) return false;

            // Check if game build version is older than Iceborne
            if (IsOlderThanIceborne(version))
            {
                return false;
            }

            // Load file
            LoadMemoryAddresses(FILE_NAME);
            GAME_VERSION = version;
            LoadValuesToMemory();
            return true;
        }

        private static void LoadMemoryAddresses(string filename)
        {
            // Clear all loaded values
            mappedAddresses.Clear();
            mappedOffsets.Clear();
            string[] fileLines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"address/{filename}"));
            foreach (string line in fileLines)
            {
                if (line.StartsWith("#")) continue; // Ignore comments
                string[] parsed = line.Split(' ');
                // parsed[0]: type
                // parsed[1]: name
                // parsed[2]: value
                AddValueToMap(parsed[0], parsed);
            }
        }

        private static void AddValueToMap(string type, string[] values)
        {
            string name = values[1];
            string value = values[2];
            switch (type)
            {
                case "Address":
                case "long":
                    long parsedValue;
                    try
                    {
                        parsedValue = ParseHex(value);
                    }
                    catch
                    {
                        Debugger.Error($"Failed parsing value for \"{name}\"");
                        parsedValue = 0xFFFFFFFF;
                    }
                    mappedAddresses.Add(name, parsedValue);
                    break;
                case "Offset":
                    string[] strOffsets = value.Split(',');
                    int[] offsets = new int[strOffsets.Length];
                    for (int i = 0; i < strOffsets.Length; i++)
                    {
                        try
                        {
                            offsets[i] = ParseHexToInt32(strOffsets[i]);
                        }
                        catch
                        {
                            Debugger.Error($"Failed to parse value {strOffsets[i]}");
                            offsets[i] = 0xFF;
                        }
                    }
                    mappedOffsets.Add(name, offsets);
                    break;
                default:
                    Debugger.Error($"Invalid type: {type}");
                    break;
            }
        }

        private static int ParseHexToInt32(string hexstring)
        {
            bool isSigned = false;
            if (hexstring.StartsWith("-")) isSigned = true;
            return Convert.ToInt32(hexstring.Replace("0x", "").Replace("-", ""), 16) * (isSigned ? -1 : 1);
        }

        private static long ParseHex(string hexstring) => Convert.ToInt64(hexstring.Replace("0x", ""), 16);

        private static void LoadValuesToMemory()
        {
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
            LoadAddressFromDict(nameof(WEAPON_MECHANICS_OFFSET), out WEAPON_MECHANICS_OFFSET, WEAPON_MECHANICS_OFFSET);
            LoadAddressFromDict(nameof(CANTEEN_OFFSET), out CANTEEN_OFFSET, CANTEEN_OFFSET);
            LoadAddressFromDict(nameof(WORLD_DATA_OFFSET), out WORLD_DATA_OFFSET, WORLD_DATA_OFFSET);
            LoadAddressFromDict(nameof(WEAPON_DATA_OFFSET), out WEAPON_DATA_OFFSET, WEAPON_DATA_OFFSET);
            LoadAddressFromDict(nameof(HUD_DATA_OFFSET), out HUD_DATA_OFFSET, HUD_DATA_OFFSET);
            // Load offsets
            LoadOffsetsFromDict("LevelOffsets", out Offsets.LevelOffsets);
            LoadOffsetsFromDict("ZoneOffsets", out Offsets.ZoneOffsets);
            LoadOffsetsFromDict("SessionOffsets", out Offsets.SessionOffsets);
            LoadOffsetsFromDict("WeaponOffsets", out Offsets.WeaponOffsets);
            LoadOffsetsFromDict("MonsterOffsets", out Offsets.MonsterOffsets);
            LoadOffsetsFromDict("EquipmentOffsets", out Offsets.EquipmentOffsets);
            LoadOffsetsFromDict("PartyOffsets", out Offsets.PartyOffsets);
            LoadOffsetsFromDict("DamageOffsets", out Offsets.DamageOffsets);
            LoadOffsetsFromDict("AbnormalityOffsets", out Offsets.AbnormalityOffsets);
            LoadOffsetsFromDict("SkillOffsets", out Offsets.SkillOffsets);
            LoadOffsetsFromDict("AbnormalityGearOffsets", out Offsets.AbnormalityGearOffsets);
            LoadOffsetsFromDict("MonsterSelectedOffsets", out Offsets.MonsterSelectedOffsets);
            LoadOffsetsFromDict("PlayerGearOffsets", out Offsets.PlayerGearOffsets);
            LoadOffsetsFromDict("PlayerLockonOffsets", out Offsets.PlayerLockonOffsets);
            LoadOffsetsFromDict("WeaponMechanicsOffsets", out Offsets.WeaponMechanicsOffsets);
            LoadOffsetsFromDict("PlayerPositionOffsets", out Offsets.PlayerPositionOffsets);
            LoadOffsetsFromDict("PlayerBasicInformationOffsets", out Offsets.PlayerBasicInformationOffsets);
            LoadOffsetsFromDict("WeaponDataOffsets", out Offsets.WeaponDataOffsets);
            LoadOffsetsFromDict("gHudStaminaBarOffsets", out Offsets.gHudStaminaBarOffsets);
            LoadOffsetsFromDict("gHudHealthBarOffsets", out Offsets.gHudHealthBarOffsets);
            // Clear addresses loaded into memory
            mappedAddresses.Clear();
            mappedOffsets.Clear();
        }

        private static void LoadAddressFromDict(string name, out long variable, long oldValue)
        {
            try
            {
                variable = mappedAddresses[name] == 0xFFFFFFFF ? oldValue : mappedAddresses[name];
            }
            catch
            {
                variable = oldValue;
                Debugger.Error($"MonsterHunterWorld.{GAME_VERSION}.map missing value for {name}");
            }
        }

        private static void LoadOffsetsFromDict(string name, out int[] offsetsArray)
        {
            try
            {
                offsetsArray = mappedOffsets[name];
            }
            catch
            {
                offsetsArray = Array.Empty<int>();
                Debugger.Error($"MonsterHunterWorld.{GAME_VERSION}.map missing offsets for {name}");
            }
        }

        // Support Iceborne and older versions
        private static bool IsOlderThanIceborne(int gameVersion)
        {
            if (gameVersion <= PREICEBORNE_VERSION)
            {
                Debugger.Error("Pre-Iceborne game not supported anymore.");
            }
            return gameVersion <= PREICEBORNE_VERSION;
        }
    }
}
