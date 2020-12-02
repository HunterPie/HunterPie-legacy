using System;
using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public struct sItem : IEquatable<sItem>
    {
        public long unk0;
        public int ItemId;
        public int Amount;

        public bool Equals(sItem other)
        {
            return Amount == other.Amount;
        }
    }
}
