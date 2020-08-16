using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential)]
    public struct sPlayerSkill
    {
        public long unk0;
        public short Level;
        public byte unk1;
        public byte unk2;
        public int unk3;
        public double unk4; // Probable not a double, but I don't know
    }
}
