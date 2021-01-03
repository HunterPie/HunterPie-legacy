using System.Collections.Generic;
using HunterPie.Memory;
using HunterPie.Core.Definitions;
using System.Linq;

namespace HunterPie.Core.Native
{
    public class MusicSkillData
    {
        private static cMusicSkillData[] data;
        public static IReadOnlyCollection<cMusicSkillData> Data => data;

        internal static void Load()
        {
            long addr = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("MUSIC_SKILL_EFC_DATA_OFFSET"),
                Address.GetOffsets("cMusicSkillEfcDataOffsets"));

            int nElements = Kernel.Read<int>(addr - 0x4);

            long[] buffer = Kernel.ReadStructure<long>(addr, nElements);
            data = new cMusicSkillData[nElements];

            int i = 0;
            foreach (long ptr in buffer)
            {
                data[i] = Kernel.ReadStructure<cMusicSkillData>(ptr);
                i++;
            }
        }

        /// <summary>
        /// Gets song data structure based on song id
        /// </summary>
        /// <param name="songId">SongId</param>
        /// <returns></returns>
        public static cMusicSkillData? GetMusicSkillData(int songId)
        {
            return data.ElementAtOrDefault(songId + 1);
        }
    }
}
