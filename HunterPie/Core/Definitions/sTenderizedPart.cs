using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential)]
    public struct sTenderizedPart
    {
        public float Timer;
        public float MaxTimer;
        public int TenderizedLevel;
        public long unk0;
        public float ExtraTimer;
        public float MaxExtraTimer;
        public int unk1;
        public int unk2;
        public int unk3;
        public int PartId;
        public int TenderizedCounter;
    }
}
