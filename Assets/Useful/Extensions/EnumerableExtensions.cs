#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace Useful.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Transforms an enumerable to provide positional indexes with the elements.
        /// </summary>
        /// <typeparam name="T">Type of the enumerated elements.</typeparam>
        /// <param name="self">The enumerable to transforms.</param>
        /// <returns>Enumerable over tuples of positional indexes and values.</returns>
        public static IEnumerable<(int index, T value)> WithIndexes<T>(this IEnumerable<T> self)
        {
            int i = 0;
            foreach (T item in self)
                yield return (i++, item);
        }

        public static T? ArgMin<T, TValue>(this IEnumerable<T> list, Func<T, TValue> mapping) where TValue : IComparable<TValue>
        {
            using var enumerator = list.GetEnumerator();
            if (!enumerator.MoveNext())
                throw new InvalidOperationException("Provided enumerable is empty.");

            T? result = enumerator.Current;
            TValue value = mapping(result);
            while (enumerator.MoveNext())
            {
                TValue v = mapping(enumerator.Current);
                if (v.CompareTo(value) < 0)
                {
                    value = v;
                    result = enumerator.Current;
                }
            }
            return result;
        }
        public static T? ArgMax<T, TValue>(this IEnumerable<T> list, Func<T, TValue> mapping) where TValue : IComparable<TValue>
        {
            using var enumerator = list.GetEnumerator();
            if (!enumerator.MoveNext())
                throw new InvalidOperationException("Provided enumerable is empty.");

            T? result = enumerator.Current;
            TValue value = mapping(result);
            while (enumerator.MoveNext())
            {
                TValue v = mapping(enumerator.Current);
                if (v.CompareTo(value) > 0)
                {
                    value = v;
                    result = enumerator.Current;
                }
            }
            return result;
        }

        public static IEnumerable<T>? EmptyToNull<T>(this IEnumerable<T> list) => list.Any() ? list : null;

        public static bool AllDistinct<T>(this IEnumerable<T> list)
        {
            var array = list.ToArray();
            return array.Distinct().Count() == array.Length;
        }
    }
}
