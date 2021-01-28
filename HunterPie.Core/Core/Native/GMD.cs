using System;
using System.Collections.Generic;
using System.Linq;
using HunterPie.Memory;
using HunterPie.Logger;
using HunterPie.Core.Definitions;
using System.Threading;
using System.Text;
using HunterPie.Utils;

namespace HunterPie.Core.Native
{
    public class GMD
    {
        #region Private
        public struct cGMD
        {
            public long gKeysBaseAddress;
            public int gKeysChunkSize;
            public int gkeysCount;
            public Dictionary<string, int> gKeys;

            public long gValuesBaseAddress;
            public int[] gValuesOffsets;
            public int gValuesChunkSize;
        }

        private static cGMD cItemsGmd = new cGMD();
        private static cGMD cBuffsGmd = new cGMD();
        private static cGMD cMonsterGmd = new cGMD();

        public static ref readonly cGMD Items => ref cItemsGmd;
        public static ref readonly cGMD Buffs => ref cBuffsGmd;
        public static ref readonly cGMD Endemic => ref cMonsterGmd;

        // BECAUSE CAPCOM IS ACTUALLY REEEEEEEEEEEEEEEEEEEEEEEEEEEEEE
        private static readonly Dictionary<string, string> specialMonsterEms = new Dictionary<string, string>(4)
        {
            {"EM032_01", "EM_TEST_00002"},
            {"EM043_05", "EM_TEST_00001"},
            {"EM120_00", "EM107_01"},
            {"EM121_00", "EM121_01"}
        };

        /// <summary>
        /// Locks current thread until all GMDs are loaded to avoid
        /// invalid values.
        /// </summary>
        /// <returns>True when all GMDs are loaded</returns>
        internal static bool InitializeGMDs()
        {
            bool ready = false;
            
            List<int> initialized = new List<int>();
            List<Func<bool>> initializers = new List<Func<bool>>()
            {
                InitializeItemsGmd,
                InitializeBuffsGmd,
                InitializeMonstersGmd
            };

            while (!ready)
            {
                int idx = 0;
                foreach (Func<bool> initializer in initializers)
                {
                    if (initializer())
                    {
                        initialized.Add(idx);
                    }
                }

                foreach (int i in initialized)
                    initializers.RemoveAt(i);

                initialized.Clear();

                if (initializers.Count == 0)
                    return true;

                Thread.Sleep(1000);
            }


            return true;
        } 

        private static bool InitializeItemsGmd()
        {
            long addr = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("GMD_ITEMS_OFFSET"),
                Address.GetOffsets("GmdItemsOffsets"));

            return LoadGMD(ref cItemsGmd, addr);
        }

        private static bool InitializeBuffsGmd()
        {
            long addr = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("GMD_BUFFS_OFFSET"),
                Address.GetOffsets("GmdOffsets"));

            return LoadGMD(ref cBuffsGmd, addr);
        }

        private static bool InitializeMonstersGmd()
        {
            long addr = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("GMD_MONSTERS_OFFSET"),
                Address.GetOffsets("GmdOffsets"));

            bool initialized = LoadGMD(ref cMonsterGmd, addr);

            if (initialized)
                LoadGMDKeys(ref cMonsterGmd);

            return initialized;
        }

        private static bool LoadGMD(ref cGMD gmd, long addr)
        {
            if (addr == Kernel.NULLPTR)
                return false;

            long addrBase = Kernel.Read<long>(addr);

            // GMD Metadata | TODO: Turn this into a structure
            string gmdName = Kernel.ReadString(addr - 0xEC, 32);
            int nElements = Kernel.Read<int>(addr - 0x40);
            gmd.gValuesChunkSize = Kernel.Read<int>(addr - 0x10);
            gmd.gKeysChunkSize = Kernel.Read<int>(addr - 0x30);
            gmd.gKeysBaseAddress = Kernel.Read<long>(addr - 0x28);
            gmd.gkeysCount = Kernel.Read<int>(addr - 0x20);

            // An array of pointers to the strings, this way we can index each element without
            // calculating the string length first
            long[] gValueStringsPtrs = Kernel.ReadStructure<long>(addrBase, nElements);

            // Calculates the offset of each string, this way we can just get the string lenght
            // and also where they start
            gmd.gValuesOffsets = gValueStringsPtrs
                .Where(ptr => ((ptr & Address.GetAddress("BASE")) == 0))
                .Select(ptr => (int)(ptr - gValueStringsPtrs[0]))
                .ToArray();

            gmd.gValuesBaseAddress = Kernel.Read<long>(addrBase);

            Debugger.Write($"[GMD] Indexed {gmdName} (Strings: {nElements} | Chunk Size: {gmd.gValuesChunkSize} Bytes)", "#FFC88DF2");

            return true;
        }

        private static void LoadGMDKeys(ref cGMD gmd)
        {
            gmd.gKeys?.Clear();

            gmd.gKeys = new Dictionary<string, int>(gmd.gkeysCount);

            char[] chars = Kernel.ReadStructure<char>(gmd.gKeysBaseAddress, gmd.gKeysChunkSize);

            
            StringBuilder sb = new StringBuilder();

            int idx = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] != '\x00')
                {
                    sb.Append(chars[i]);
                } else
                {
                    gmd.gKeys.Add(sb.ToString(), idx);
                    idx++;
                    sb.Clear();
                    continue;
                }
            }
        }

        #endregion

        #region Public GMD API
        /// <summary>
        /// Gets item name in the language the game is currently using
        /// </summary>
        /// <param name="itemId">Item Id, you get it from: <seealso cref="sItem.ItemId"/></param>
        /// <returns>The item name</returns>
        public static string GetItemNameById(int itemId)
        {
            if (itemId * 2 >= cItemsGmd.gValuesOffsets.Length)
                return null;

            return GetRawValueString(cItemsGmd, itemId * 2);
        }

        /// <summary>
        /// Gets item description in the language the game is currently using
        /// </summary>
        /// <param name="itemId">Item id</param>
        /// <returns>The item description</returns>
        public static string GetItemDescriptionById(int itemId)
        {
            char[] junk = { ' ', '\x0A', '\x0B', '\x0C', '\x0D'};
            string rawDescription = GetRawValueString(cItemsGmd, itemId * 2 + 1);

            // Apparently this is faster than using Regex to replace
            string[] temp = rawDescription.Split(junk, StringSplitOptions.RemoveEmptyEntries);

            return GetRawValueString(cItemsGmd, itemId * 2 + 1).RemoveChars();
        }

        /// <summary>
        /// Gets monster name by Em
        /// </summary>
        /// <param name="monsterEm">Monster em</param>
        /// <returns>Monster name</returns>
        public static string GetMonsterNameByEm(string monsterEm)
        {
            if (monsterEm is null)
                return null;

            // Capcom, if you're reading this
            // WHY????? JUST WHY??????
            monsterEm = monsterEm.ToUpperInvariant();

            if (!cMonsterGmd.gKeys.ContainsKey(monsterEm))
            {
                if (specialMonsterEms.ContainsKey(monsterEm))
                    monsterEm = specialMonsterEms[monsterEm];
                else
                {
                    monsterEm = monsterEm.Split('_').First();

                    if (!cMonsterGmd.gKeys.ContainsKey(monsterEm))
                        return null;
                }
            }

            int idx = cMonsterGmd.gKeys[monsterEm];
            return GetRawValueString(cMonsterGmd, idx);
        }

        /// <summary>
        /// Get monster description by their Em
        /// </summary>
        /// <param name="monsterEm">Monster em</param>
        /// <returns>Monster description</returns>
        public static string GetMonsterDescriptionByEm(string monsterEm)
        {
            if (monsterEm is null)
                return null;

            monsterEm = monsterEm.ToUpperInvariant();

            if (!cMonsterGmd.gKeys.ContainsKey(monsterEm))
            {
                if (specialMonsterEms.ContainsKey(monsterEm))
                    monsterEm = specialMonsterEms[monsterEm];
                else
                {
                    monsterEm = monsterEm.Split('_').First();

                    if (!cMonsterGmd.gKeys.ContainsKey(monsterEm))
                        return null;
                }
                
            }

            monsterEm += "_DESC";
            int idx = cMonsterGmd.gKeys[monsterEm];
            return GetRawValueString(cMonsterGmd, idx).RemoveChars();
        }

        /// <summary>
        /// Returns song name based on its Id
        /// </summary>
        /// <param name="songId">Song Id</param>
        /// <returns>Song name</returns>
        public static string GetMusicSkillNameById(int songId)
        {

            cMusicSkillData? data = MusicSkillData.GetMusicSkillData(songId);

            if (data is null)
                return null;

            songId = ((cMusicSkillData)data).StringId;

            return GetRawValueString(cBuffsGmd, songId);
        }

        /// <summary>
        /// Gets raw string from a GMD file
        /// </summary>
        /// <param name="gmd">the GMD that will be searched for</param>
        /// <param name="idx">Index in the value</param>
        /// <returns>Raw string read from memory</returns>
        public static string GetRawValueString(cGMD gmd, int idx)
        {
            try
            {
                long length;
                if ((idx + 1) >= gmd.gValuesOffsets.Length)
                {
                    length = gmd.gValuesChunkSize - gmd.gValuesOffsets[idx];
                }
                else
                {
                    length = gmd.gValuesOffsets[idx + 1] - gmd.gValuesOffsets[idx];
                }

                long stringAddress = gmd.gValuesBaseAddress + gmd.gValuesOffsets[idx];

                return Kernel.ReadString(stringAddress, (int)length);
            }catch
            {
                return "Unknown";
            }
        }
        #endregion
    }
}
