using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential, Size = 0x18)]
    public struct sItem
    {
        long unk0;
        public int ItemId;
        public int Amount;
    }
}
