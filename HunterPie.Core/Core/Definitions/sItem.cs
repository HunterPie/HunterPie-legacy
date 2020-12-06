using System;
using System.Collections.Generic;
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
            return ItemId == other.ItemId && Amount == other.Amount;
        }
    }

    public class sItemEqualityComparer : IEqualityComparer<sItem>
    {
        public bool Equals(sItem a, sItem b)
        {
            return a.ItemId == b.ItemId;
        }

        public int GetHashCode(sItem obj)
        {
            return obj.ItemId.GetHashCode();
        }
    }
}
