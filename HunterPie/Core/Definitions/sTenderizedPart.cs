using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential)]
    public struct sTenderizedPart
    {
        public long Address;
        public float Duration;
        public float MaxDuration;
        public int unk0;
        public int unk1;
        public long unk2;
        public float ExtraDuration;
        public float MaxExtraDuration;
        public int unk3;
        public int unk4;
        public uint PartId;
        public int TenderizedCounter;
        public int unk6;
        public int unk7;
    }
}
