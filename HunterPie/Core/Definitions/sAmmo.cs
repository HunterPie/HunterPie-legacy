using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential)]
    public struct sAmmo
    {
        public long unk0;
        public int Ammo;
        public int Total;
        public int Maximum;
        public int unk1; // Always 99?
    }
}
