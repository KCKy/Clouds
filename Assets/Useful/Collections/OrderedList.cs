using System;
using System.Collections;
using System.Collections.Generic;

namespace Useful.Collections
{
    public class OrderedList<TKey, TElement> : IEnumerable<OrderedList<TKey, TElement>.Entry>
    {
        public readonly struct Entry
        {
            public readonly TKey Key;
            public readonly TElement Element;

            public Entry(TKey key, TElement element)
            {
                Key = key;
                Element = element;
            }

            public void Deconstruct(out TKey key, out TElement element)
            {
                key = Key;
                element = Element;
            }
        }
        class Section
        {
            public LinkedListNode<Entry> Start;
            public LinkedListNode<Entry> End;

            public Section(LinkedListNode<Entry> start, LinkedListNode<Entry> end)
            {
                this.Start = start;
                this.End = end;
            }
        }

        public int Count => _elementNodes.Count;

        readonly LinkedList<Entry> _list = new();
        readonly SortedList<TKey, Section> _sections = new();
        readonly Dictionary<TElement, LinkedListNode<Entry>> _elementNodes = new();

        public void Add(TKey key, TElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (_elementNodes.ContainsKey(element))
                throw new ArgumentException("This list already contains the specified element");

            Entry e = new(key, element);

            LinkedListNode<Entry> node;
            if (!_sections.TryGetValue(key, out Section section))
            {
                Section s = new(null, null);
                _sections.Add(key, s);
                int sectionIndex = _sections.IndexOfKey(key);
                node = sectionIndex == 0 ? _list.AddFirst(e) : _list.AddAfter(_sections.Values[sectionIndex - 1].End, e);
                s.Start = node;
                s.End = node;
            }
            else
            {
                node = _list.AddAfter(section.End, e);
                section.End = node;
            }
            _elementNodes.Add(element, node);
        }

        public void Remove(TElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (!_elementNodes.Remove(element, out var node))
                throw new ArgumentException("This list does not contain the specified element");

            TKey key = node.Value.Key;
            Section s = _sections[key];
            if (s.Start == node && s.End == node)
                _sections.Remove(key);
            else if (s.Start == node)
                s.Start = node.Next;
            else if (s.End == node)
                s.End = node.Previous;

            _list.Remove(node);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<Entry> GetEnumerator() => _list.GetEnumerator();
    }
}
