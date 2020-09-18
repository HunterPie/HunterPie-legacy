using System;
using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential)]
    public struct sEquippedAmmo : IEquatable<sEquippedAmmo>
    {
        public long unk0;
        public int index;
        public int index2;
        public int ammoToShoot; // Index of the ammo it is shooting
        public int ammoInBarrel;// I think? It only updates after reloding sometimes
        public int unk1;
        public int ItemId;      // Ammo id

        public bool Equals(sEquippedAmmo other)
        {
            return index == other.index;
        }
    }
}
