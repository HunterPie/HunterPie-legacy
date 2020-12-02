using System.Threading;

namespace HunterPie.Memory
{
    public class BufferPool<T> where T : struct
    {
        private const int resetLimit = 2000000000;
        private static readonly object limitLock = new object();
        private readonly int capacity;
        private readonly T[][] buffers;
        private int current;

        public BufferPool(int capacity, int bufferSize = 1)
        {
            this.capacity = capacity;
            buffers = new T[capacity][];
            for (int i = 0; i < capacity; i++)
            {
                buffers[i] = new T[bufferSize];
            }
        }

        public T[] Get()
        {
            int index = Interlocked.Increment(ref current) % capacity;
            if (current > resetLimit)
            {
                lock (limitLock)
                {
                    if (current > resetLimit)
                    {
                        current -= resetLimit - resetLimit % capacity;
                    }
                }
            }

            return buffers[index];
        }
    }
}
