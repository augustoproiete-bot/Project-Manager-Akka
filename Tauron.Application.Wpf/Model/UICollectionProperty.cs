using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.Model
{
    [PublicAPI]
    public class UICollectionProperty<TData> : IList<TData>, IReadOnlyList<TData>
    {
        private ObservableCollection<TData>? _collection;
        public UIProperty<ObservableCollection<TData>> Property { get; }

        public bool IsNull => _collection == null;

        public UICollectionProperty(UIProperty<ObservableCollection<TData>> property)
        {
            Property = property;
            _collection = property.Value;
            property.PropertyValueChanged += () => _collection = property.Value;
        }

        public IEnumerator<TData> GetEnumerator() => _collection?.GetEnumerator() ?? (IEnumerator<TData>)Array.Empty<TData>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(TData item) => _collection?.Add(item);

        public void Clear() => _collection?.Clear();

        public bool Contains(TData item) => _collection?.Contains(item) ?? false;

        public void CopyTo(TData[] array, int arrayIndex) => _collection?.CopyTo(array, arrayIndex);

        public bool Remove(TData item) => _collection?.Remove(item) ?? false;

        int ICollection<TData>.Count => _collection?.Count ?? -1;

        public bool IsReadOnly => (_collection as IList<TData>)?.IsReadOnly ?? true;

        public int IndexOf(TData item) => _collection?.IndexOf(item) ?? -1;

        public void Insert(int index, TData item) => _collection?.Insert(index, item);

        public void RemoveAt(int index) => _collection?.RemoveAt(index);

        public TData this[int index]
        {
            get => _collection != null ? _collection[index] : default!;
            set
            {
                if(_collection == null) return;
                _collection[index] = value;
            }
        }

        int IReadOnlyCollection<TData>.Count => _collection?.Count ?? -1;
    }
}