using System;

namespace HunterPie.Utils
{
    public static class GenericExtensions
    {
        public static bool IsWithin<T>(this T value, T low, T high) where T : IComparable<T>
        {
            return value.CompareTo(low) > 0 && value.CompareTo(high) < 0;
        }
    }
}
