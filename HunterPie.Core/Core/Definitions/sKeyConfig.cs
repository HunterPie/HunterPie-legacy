using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential, Size = 0x18)]
    public struct sKeyConfig
    {
        public uint MainKey;
        public uint SubKey;
        public long unk0;
        public int NumberOfKeys;
        public int unk1;
    }
}
