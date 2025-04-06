using System;
using System.Collections.Generic;
using System.Linq;
using Useful.Random;

namespace Useful.Extensions
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> list, IRandom random = null)
        {
            random ??= UnityRandomAdapter.Instance;

            int n = list.Count;  
            while (n > 1)
            {  
                n--;  
                int k = random.NextInt(n + 1);  
                (list[k], list[n]) = (list[n], list[k]);
            }  
        }

        public static T PickRandom<T>(this IList<T> list, IRandom random = null)
        {
            random ??= UnityRandomAdapter.Instance;
            return list[random.NextInt(list.Count)];
        }

        public static T RemoveRandom<T>(this IList<T> list, IRandom random = null)
        {
            random ??= UnityRandomAdapter.Instance;
            int index = random.NextInt(list.Count);
            T item = list[index];
            list.RemoveAt(index);
            return item;
        }

        public static IEnumerable<List<T>> Permutations<T>(this IList<T> source)
        {
            if (source.Count <= 1)
                yield return new(source);

            foreach (T item in source)
            {
                List<T> x = new(source);
                x.Remove(item);
                foreach (var permutation in x.Permutations())
                {
                    permutation.Add(item);
                    yield return permutation;
                }
            }
        }

        public static void Fill<T>(this IList<T> list, Func<T> getValue)
        {
            for (int i = 0; i < list.Count; i++)
                list[i] = getValue();
        }

        public static void Fill<T>(this IList<T> list, T value)
        {
            for (int i = 0; i < list.Count; i++)
                list[i] = value;
        }

        public static List<T> PickOut<T>(this IList<T> list, IEnumerable<int> indexes)
        {
            return indexes.Select(i => list[i]).ToList();
        }
    }
}
