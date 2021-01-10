using System;

namespace HunterPie.Utils
{
    public static class StringExtensions
    {

        /// <summary>
        /// Removes specific characters from a string
        /// </summary>
        /// <param name="value">string</param>
        /// <param name="chars">characters to remove (default is ' ', '\x0A', '\x0B', '\x0C', '\x0D')</param>
        /// <returns>Pretty string</returns>
        public static string RemoveChars(this string value, char[] chars = null)
        {
            if (chars is null)
                chars = new char[]{ ' ', '\x0A', '\x0B', '\x0C', '\x0D' };

            // Apparently this is faster than using Regex to replace
            string[] temp = value.Split(chars, StringSplitOptions.RemoveEmptyEntries);

            return string.Join(" ", temp);
        }

    }
}
