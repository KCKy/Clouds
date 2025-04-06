
using Useful.Math;

namespace Useful.Collections
{
    public class FixedCapacityStack<T>
    {
        readonly T[] _array;
        readonly int _capacity;
        int _index;

        public int Count { get; private set; }

        public FixedCapacityStack(int capacity)
        {
            _array = new T[capacity];
            _capacity = capacity;
            Count = 0;
            _index = -1;
        }

        public void Push(T item)
        {
            _index = (_index + 1) % _capacity;
            _array[_index] = item;
            if (Count < _capacity) Count++;
        }
        public T Pop()
        {
            if (Count == 0)
                throw new System.InvalidOperationException("Cannot pop from an empty stack");
            T r = _array[_index];
            _index = MathUtils.Mod(_index - 1, _capacity);
            Count--;
            return r;
        }
    }
}
