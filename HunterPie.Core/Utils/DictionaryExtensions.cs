using System;
using System.Collections.Generic;
using System.Linq;

namespace HunterPie.Utils
{
    public static class DictionaryExtensions
    {
        public static bool IsEqualTo<TKey, TValue>(this Dictionary<TKey, TValue> self, Dictionary<TKey, TValue> other) where TValue : IEquatable<TValue>
        {
            if (self.Count != other.Count)
                return false;

            return self.Where(pair => other.ContainsKey(pair.Key) && other[pair.Key]
            .Equals(pair.Value)).Count() == self.Count;
        }
    }
}
