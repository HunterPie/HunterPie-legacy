using System.Runtime.InteropServices;
using HunterPie.Core.Enums;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential)]
    public struct sFoodData
    {
        public ulong cRef;
        public int HealthType;
        public int StaminaType;
        public FoodBuffType AttackType;
        public FoodBuffType DefenseType;
        public FoodBuffType ElementalType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public FoodSkills[] Skills;
    }
}
