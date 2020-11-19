using System.Runtime.InteropServices;
using HunterPie.Core.Enums;

namespace HunterPie.Core.Definitions
{

    [StructLayout(LayoutKind.Sequential, Size = 0x8)]
    public struct sSharpness
    {
        public int Sharpness;
        public SharpnessLevel Level;
    }
}
