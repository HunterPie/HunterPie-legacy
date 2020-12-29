using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential)]
    public struct sMonsterModelData
    {
        // 0x160 - 0x168
        public sVector3 pos;
        // 0x16C
        public float unk0;
        // 0x170
        public float unk1;
        // 0x174
        public float unk2;
        public float verticalAngle;
        public float w;
        public sVector3 scale;
    }
}
