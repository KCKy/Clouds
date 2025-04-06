using System;
using System.Collections;
using System.Collections.Generic;

namespace Useful.Random
{
    /// <summary>
    /// Represents an unordered set of items of type T, being able to return them in a random order.
    /// </summary>
    public class RandomSet<T> : ICollection<T>
    {
        // underlying container of items
        readonly List<T> _list;
        // maps items to their position in list_
        readonly Dictionary<T, int> _positions;
        readonly IRandom _random;

        public int Count => _list.Count;
        public bool IsReadOnly => false;

        /// <summary>
        /// Creates a new empty instance of <see cref="RandomSet{T}"/>.
        /// </summary>
        /// <param name="random">Random number generator to read from.</param>
        public RandomSet(IRandom random = null)
        {
            _list = new();
            _positions = new();
            _random = random ?? UnityRandomAdapter.Instance;
        }

        /// <summary>
        /// Creates a new instance of <see cref="RandomSet{T}"/>, filled with the items from the provided container.
        /// </summary>
        /// <param name="items">The container to take items from.</param>
        /// <param name="random">Random number generator to read from.</param>
        public RandomSet(IEnumerable<T> items, IRandom random) : this(random)
        {
            foreach (T item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Adds a new item to the set, if not present.
        /// </summary>
        public void Add(T item)
        {
            if (_positions.ContainsKey(item))
                return;
            _positions.Add(item, _list.Count);
            _list.Add(item);
        }

        /// <summary>
        /// Copies the elements in an arbitrary order (not random) to an array, starting at the given array index.
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        /// <summary>
        /// Removes an item from the set, if present.
        /// </summary>
        /// <returns>true, if the item was present, false otherwise.</returns>
        public bool Remove(T item)
        {
            if (!_positions.Remove(item, out int pos))
                return false;
            if (pos != _list.Count - 1)
            {
                _list[pos] = _list[^1];
                _positions[_list[pos]] = pos;
            }
            _list.RemoveAt(_list.Count - 1);
            return true;
        }

        /// <summary>
        /// Adds multiple items to the set, skipping those that are already present.
        /// </summary>
        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Gets a random item from the set and removes it. Throws an exception when the set is empty.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public T PopRandom()
        {
            if (_list.Count == 0)
                throw new InvalidOperationException("Cannot pop from an empty set");
            int r = _random.NextInt(_list.Count);
            T ret = _list[r];
            Remove(ret);
            return ret;
        }

        /// <summary>
        /// Tests whether an item is present in the set.
        /// </summary>
        public bool Contains(T item) => _positions.ContainsKey(item);

        /// <summary>
        /// Removes all items.
        /// </summary>
        public void Clear()
        {
            _list.Clear();
            _positions.Clear();
        }

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
