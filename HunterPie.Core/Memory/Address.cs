using System;
using System.Collections.Generic;
using System.IO;
using HunterPie.Logger;

namespace HunterPie.Memory
{
    public class Offsets
    {
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
    public class Address
    {
        
        public const int PREICEBORNE_VERSION = 168031;
        public static int GAME_VERSION = 168031;

        private static readonly Dictionary<string, long> addresses = new Dictionary<string, long>();
        private static readonly Dictionary<string, int[]> offsets = new Dictionary<string, int[]>();

        /// <summary>
        /// Adds a static address to the Addresses dictionary
        /// </summary>
        /// <param name="name">Custom name</param>
        /// <param name="address">Address</param>
        /// <returns>Whether the value was added succesfully</returns>
        public static bool AddAddress(string name, long address)
        {
            if (addresses.ContainsKey(name))
            {
                return false;
            } else
            {
                addresses.Add(name, address);
                return true;
            }
        }

        /// <summary>
        /// Adds a offsets array to the Offsets dictionary
        /// </summary>
        /// <param name="name">Custom name</param>
        /// <param name="offsetsArr">Offsets</param>
        /// <returns>Whether the value was added successfully</returns>
        public static bool AddOffsets(string name, int[] offsetsArr)
        {
            if (offsets.ContainsKey(name))
            {
                return false;
            } else
            {
                offsets.Add(name, offsetsArr);
                return true;
            }
        }

        public static IReadOnlyDictionary<string, long> Addresses => addresses;
        public static IReadOnlyDictionary<string, int[]> Offsets => offsets;

        // Consts
        public const long CooldownFixed = 0x9EC;
        public const long CooldownDynamic = 0x99C;
        public const long TimerFixed = 0xADC;
        public const long TimerDynamic = 0xA8C;

        public static bool LoadMemoryMap(int version)
        {
            string FILE_NAME = $"MonsterHunterWorld.{version}.map";

            // If dir or file doesn't exist
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "address")))
                return false;

            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"address/{FILE_NAME}")))
                return false;

            // Check if game build version is older than Iceborne
            if (IsOlderThanIceborne(version))
            {
                return false;
            }
            
            // Load file
            LoadMemoryAddresses(FILE_NAME);
            GAME_VERSION = version;
            
            return true;
        }

        private static void LoadMemoryAddresses(string filename)
        {
            // Clear all loaded values
            addresses.Clear();
            offsets.Clear();

            string[] fileLines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"address/{filename}"));
            foreach (string line in fileLines)
            {
                if (line.StartsWith("#"))
                    continue; // Ignore comments

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
                    addresses.Add(name, parsedValue);
                    break;
                case "Offset":
                    string[] strOffsets = value.Split(',');
                    int[] parsedOffsets = new int[strOffsets.Length];
                    for (int i = 0; i < strOffsets.Length; i++)
                    {
                        try
                        {
                            parsedOffsets[i] = ParseHexToInt32(strOffsets[i]);
                        }
                        catch
                        {
                            Debugger.Error($"Failed to parse value {strOffsets[i]}");
                            parsedOffsets[i] = 0xFF;
                        }
                    }
                    offsets.Add(name, parsedOffsets);
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
