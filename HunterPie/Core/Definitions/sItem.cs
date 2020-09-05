using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public struct sItem
    {
        public long unk0;
        public int ItemId;
        public int Amount;
    }
}
