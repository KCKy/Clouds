using System.Collections.Generic;

namespace Useful.Extensions
{
    public static class DictionaryExtensions
    {
        public static void Increment<T>(this IDictionary<T, int> dict, T key, int value = 1)
        {
            if (!dict.TryAdd(key, value))
                dict[key] += value;
        }
    }
}
