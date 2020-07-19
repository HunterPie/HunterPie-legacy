using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential)]
    public struct sMonsterStatus
    {
        public long Source; // Monster Address
        public long unk0;
        public int unk1;
        public int IsActive;
        public float Buildup;
        public float DamageDone; // Not sure why this is a thing,
        public float unk3;
        public float Duration;
        public float MaxDuration;
        public int unk4;
        public int unk5;
        public uint Counter;
        public int unk6;
        public float MaxBuildup;
        public float unk8; // 10 for legiana
        public float unk9; // 10 for legiana

        public static sMonsterAilment Convert(sMonsterStatus obj)
        {
            sMonsterAilment ailment = new sMonsterAilment
            {
                Source = obj.Source,
                IsActive = obj.IsActive,
                MaxDuration = obj.MaxDuration,
                Duration = obj.Duration,
                MaxBuildup = obj.MaxBuildup,
                Buildup = obj.Buildup,
                Counter = obj.Counter
            };

            return ailment;
        }
    }
}
