using System;
using System.Collections.Generic;

namespace HunterPie.Memory
{
    public static class Buffers
    {
        private static int capacity;
        private static Dictionary<Type, object> buffers;
        private static readonly object lockObject = new object();

        public static void Initialize(int commonCapacity)
        {
            capacity = commonCapacity;
            buffers = new Dictionary<Type, object>();
        }

        public static void Add<T>(int size = 1) where T : struct => buffers.Add(typeof(T), new BufferPool<T>(capacity, size));

        public static T[] Get<T>() where T : struct
        {
            if (!buffers.ContainsKey(typeof(T)))
            {
                lock (lockObject)
                {
                    if (!buffers.ContainsKey(typeof(T)))
                    {
                        Add<T>();
                    }
                }
            }

            BufferPool<T> pool = (BufferPool<T>)buffers[typeof(T)];
            return pool.Get();
        }
    }
}
