namespace HunterPie.Core.Enums
{
    public class Helper
    {
        public static string ConvertAbnormalityType(AbnormalityType type)
        {
            switch (type)
            {
                case AbnormalityType.HuntingHorn:
                    return "HUNTINGHORN";
                case AbnormalityType.Palico:
                    return "PALICO";
                case AbnormalityType.Debuff:
                    return "DEBUFF";
                case AbnormalityType.Misc:
                    return "MISC";
                case AbnormalityType.Gear:
                    return "GEAR";
                default:
                    return null;
            }
        }
    }
}
