using System;
using System.Collections;
using System.Collections.Generic;

namespace Useful.Random
{
    /// <summary>
    /// Represents an unordered set of weighted items of type T, being able to return them in a random order. Each item's chance to be returned is proportional to its weight.
    /// </summary>
    public class WeightedRandomSet<T> : IEnumerable<(T item, float weight)>
    {
        // underlying container of items
        readonly List<(T item, float weight)> _list;
        // maps items to their position in list_
        readonly Dictionary<T, int> _positions;
        readonly IRandom _random;
        public int Count => _list.Count;

        /// <summary>
        /// Creates a new empty instance of <see cref="WeightedRandomSet{T}"/>.
        /// </summary>
        /// <param name="random">Random number generator to read from.</param>
        public WeightedRandomSet(IRandom random = null)
        {
            _list = new();  
            _positions = new();
            _random = random ?? UnityRandomAdapter.Instance;
        }

        /// <summary>
        /// Creates a new instance of <see cref="WeightedRandomSet{T}"/>, filled with the item-weight pairs from the provided container.
        /// </summary>
        /// <param name="items">The container to take items and weights from.</param>
        /// <param name="random">Random number generator to read from.</param>
        public WeightedRandomSet(IEnumerable<(T item, float weight)> items, IRandom random) : this(random)
        {
            foreach ((T item, float weight) in items)
            {
                AddOrUpdate(item, weight);
            }
        }

        /// <summary>
        /// Adds a new item to the set if not present, otherwise updates the item's weight.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="weight">Positive weight. The greater weight, the greater chance to be selected.</param>
        public void AddOrUpdate(T item, float weight)
        {
            if (weight <= 0)
                throw new ArgumentOutOfRangeException(nameof(weight), weight, "Weight must be positive");
            if (_positions.ContainsKey(item))
            {
                UpdateWeight(item, weight);
                return;
            }
            _positions.Add(item, _list.Count);
            _list.Add((item, weight));
        }
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
                _positions[_list[pos].item] = pos;
            }
            _list.RemoveAt(_list.Count - 1);
            return true;
        }
        /// <summary>
        /// Gets a random item from the set and removes it. Throws an exception when the set is empty.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public T PopRandom()
        {
            if (Count == 0)
                throw new InvalidOperationException("Cannot pop from an empty set");
            float totalWeight = 0;
            for (int i = 0; i < Count; i++)
                totalWeight += _list[i].weight;
            float r = _random.NextFloat(0, totalWeight);
            int pos = 0;
            for (int i = 0; i < Count; i++)
            {
                r -= _list[i].weight;
                if (r >= 0)
                    continue;
                pos = i;
                break;
            }
            T ret = _list[pos].item;
            Remove(ret);
            return ret;
        }

        /// <summary>
        /// Changes the weight of an item already in the set.
        /// </summary>
        /// <param name="item">Item to update. Throws an exception when the item isn't present.</param>
        /// <param name="newWeight">Positive weight. The greater weight, the greater chance to be selected.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void UpdateWeight(T item, float newWeight)
        {
            if (newWeight <= 0)
                throw new ArgumentOutOfRangeException(nameof(newWeight), newWeight, "Weight must be positive");
            if (!_positions.TryGetValue(item, out int pos))
                throw new InvalidOperationException($"Item {item} was not present in the set");
            _list[pos] = (item, newWeight);
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
        public IEnumerator<(T item, float weight)> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
