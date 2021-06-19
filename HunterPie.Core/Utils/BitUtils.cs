namespace HunterPie.Utils
{
    public static class BitUtils
    {
        /// <summary>
        /// Brian Kernighan’s Algorithm to count set bits in an integer
        /// </summary>
        /// <param name="n">The integer whose bits we want to count</param>
        public static byte CountBits(long n)
        {
            byte count = 0;
            while (n != 0)
            {
                n &= (n - 1);
                count++;
            }
            return count;
        }
    }
}
