using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Akka.Code.Configuration.Converter;

namespace Akka.Code.Configuration.Elements
{
    public sealed class ArrayElement<TType> : IList<TType>
    {
        private readonly ConverterBase _converter;
        private readonly List<TType> _elements = new List<TType>();

        public ArrayElement()
        {
            _converter = ConverterBase.Find(typeof(TType));
        }

        public IEnumerator<TType> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _elements).GetEnumerator();
        }

        public void Add(TType item)
        {
            _elements.Add(item);
        }

        public void Clear()
        {
            _elements.Clear();
        }

        public bool Contains(TType item)
        {
            return _elements.Contains(item);
        }

        public void CopyTo(TType[] array, int arrayIndex)
        {
            _elements.CopyTo(array, arrayIndex);
        }

        public bool Remove(TType item)
        {
            return _elements.Remove(item);
        }

        public int Count => _elements.Count;

        public bool IsReadOnly => ((IList<TType>) _elements).IsReadOnly;

        public int IndexOf(TType item)
        {
            return _elements.IndexOf(item);
        }

        public void Insert(int index, TType item)
        {
            _elements.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _elements.RemoveAt(index);
        }

        public TType this[int index]
        {
            get => _elements[index];
            set => _elements[index] = value;
        }

        public override string ToString()
        {
            return $"[{string.Join(", ", _elements.Select(e => _converter.ToElementValue(e)).Where(s => !string.IsNullOrEmpty(s)))}]";
        }
    }
}