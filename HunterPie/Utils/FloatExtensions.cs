namespace HunterPie.Utils
{
    public static class FloatExtensions
    {
        public static bool IsWithin(this float value, float low, float high)
        {
            return value >= low && value <= high;
        }
    }
}
