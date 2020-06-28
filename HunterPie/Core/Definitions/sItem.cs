using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential)]
    public struct sItem
    {
        readonly long unk0;
        public int ItemId;
        public int Amount;
    }
}
