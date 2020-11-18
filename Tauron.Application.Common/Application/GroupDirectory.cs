using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Tauron.Application
{
    public sealed class ImmutableGroupDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, IImmutableList<TValue>>>, IDictionary<TKey, IImmutableList<TValue>>
        where TKey : notnull
    {
        public static ImmutableGroupDictionary<TKey, TValue> Empty => new();
        
        private readonly ImmutableDictionary<TKey, IImmutableList<TValue>> _data;
        private readonly Func<IImmutableList<TValue>>                      _listFactory;

        private ImmutableGroupDictionary()
        {
            _data = ImmutableDictionary<TKey, IImmutableList<TValue>>.Empty;
            _listFactory = () => ImmutableList<TValue>.Empty;
        }

        private ImmutableGroupDictionary(ImmutableDictionary<TKey, IImmutableList<TValue>> data, Func<IImmutableList<TValue>> listFactory)
        {
            _data        = data;
            _listFactory = listFactory;
        }

        private Exception NotSupported()
            => new NotSupportedException("Immutable Dictionary");
        
        public IEnumerator<KeyValuePair<TKey, IImmutableList<TValue>>> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        bool ICollection<KeyValuePair<TKey, IImmutableList<TValue>>>.Remove(KeyValuePair<TKey, IImmutableList<TValue>> item) 
            => throw NotSupported();

        public int                                                   Count      => _data.Count;

        bool ICollection<KeyValuePair<TKey, IImmutableList<TValue>>>.IsReadOnly => true;

        void IDictionary<TKey, IImmutableList<TValue>>.Add(TKey key, IImmutableList<TValue> value) 
            => throw NotSupported();

        public bool                                    ContainsKey(TKey key) => _data.ContainsKey(key);
        bool IDictionary<TKey, IImmutableList<TValue>>.Remove(TKey      key) 
            => throw NotSupported();

        public bool TryGetValue(TKey key, [NotNullWhen(true)]out IImmutableList<TValue>? value) 
            => _data.TryGetValue(key, out value);

        IImmutableList<TValue> IDictionary<TKey, IImmutableList<TValue>>.this[TKey key]
        {
            get => _data[key];
            set => throw NotSupported();
        }

        public IImmutableList<TValue> this[TKey key] => _data[key];

        public IEnumerable<TKey>  Keys => _data.Keys                                

        ICollection<IImmutableList<TValue>> IDictionary<TKey, IImmutableList<TValue>>.Values => _data.Values.ToArray();

        ICollection<TKey> IDictionary<TKey, IImmutableList<TValue>>.Keys => _data.Keys.ToArray();

        public IEnumerable<IImmutableList<TValue>>                Values                                      => _data.Values;
        
        public ImmutableGroupDictionary<TKey, TValue> Add(TKey key, IImmutableList<TValue> value) 
            => GenericAdd(key, value);

        public ImmutableGroupDictionary<TKey, TValue> Add(TKey key, TValue value) 
            => GenericAdd(key, new []{ value });
        
        public ImmutableGroupDictionary<TKey, TValue> Add(TKey key) 
            => GenericAdd(key, Enumerable.Empty<TValue>());

        private ImmutableGroupDictionary<TKey, TValue> GenericAdd(TKey key, IEnumerable<TValue> values)
        {
            
        }

        public ImmutableGroupDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, IImmutableList<TValue>>> pairs) 
            => pairs.Aggregate(this, (current, pair) => current.GenericAdd(pair.Key, pair.Value));

        public ImmutableGroupDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> pairs) 
            => pairs.Aggregate(this, (current, pair) => current.GenericAdd(pair.Key, pair.Value));

        void ICollection<KeyValuePair<TKey, IImmutableList<TValue>>>.Add(KeyValuePair<TKey, IImmutableList<TValue>> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<TKey, IImmutableList<TValue>>>.Clear()
        {
            throw new NotImplementedException();
        }

        public IImmutableDictionary<TKey, IImmutableList<TValue>>    Clear() => throw new NotImplementedException();

        public bool                                                  Contains(KeyValuePair<TKey, IImmutableList<TValue>> pair) => throw new NotImplementedException();
        void ICollection<KeyValuePair<TKey, IImmutableList<TValue>>>.CopyTo(KeyValuePair<TKey, IImmutableList<TValue>>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IImmutableDictionary<TKey, IImmutableList<TValue>> Remove(TValue key) => throw new NotImplementedException();
        
        public IImmutableDictionary<TKey, IImmutableList<TValue>> Remove(TKey key) => throw new NotImplementedException();

        public IImmutableDictionary<TKey, IImmutableList<TValue>> RemoveRange(IEnumerable<TKey> keys) => throw new NotImplementedException();

        public IImmutableDictionary<TKey, IImmutableList<TValue>> SetItem(TKey key, IImmutableList<TValue> value) => throw new NotImplementedException();

        public IImmutableDictionary<TKey, IImmutableList<TValue>> SetItem(TKey key, TValue value) => throw new NotImplementedException();
        
        public IImmutableDictionary<TKey, IImmutableList<TValue>> SetItems(IEnumerable<KeyValuePair<TKey, IImmutableList<TValue>>> items) => throw new NotImplementedException();

        public bool TryGetKey(TKey equalKey, out TKey actualKey) => throw new NotImplementedException();
    }

    [Serializable]
    [PublicAPI]
    [DebuggerStepThrough]
    public class GroupDictionary<TKey, TValue> : Dictionary<TKey, ICollection<TValue>>
        where TKey : notnull
        //where TKey : class where TValue : class
    {
        private readonly Type _listType;

        private Type? _genericTemp;

        public GroupDictionary(Type listType) 
            => _listType = Argument.NotNull(listType, nameof(listType));

        public GroupDictionary() 
            => _listType = typeof(List<TValue>);

        public GroupDictionary(bool singleList) 
            => _listType = singleList ? typeof(HashSet<TValue>) : typeof(List<TValue>);

        public GroupDictionary(GroupDictionary<TKey, TValue> groupDictionary)
            : base(groupDictionary)
        {
            _listType = groupDictionary._listType;
            _genericTemp = groupDictionary._genericTemp;
        }

        protected GroupDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info.GetValue("listType", typeof(Type)) is not Type listType)
                throw new InvalidOperationException("List Type not in Serialization info");

            _listType = listType;
        }

        public ICollection<TValue> AllValues => new AllValueCollection(this);

        public new ICollection<TValue> this[TKey key]
        {
            get
            {
                if (!ContainsKey(key)) Add(key);
                return base[key];
            }

            set => base[key] = value;
        }

        private object CreateList()
        {
            if (!typeof(ICollection<TValue>).IsAssignableFrom(_listType)) throw new InvalidOperationException();

            if (_genericTemp != null) return Activator.CreateInstance(_genericTemp) ?? throw new InvalidOperationException("List Creation Failed");

            if (_listType.ContainsGenericParameters)
            {
                if (_listType.GetGenericArguments().Length != 1) throw new InvalidOperationException();

                _genericTemp = _listType.MakeGenericType(typeof(TValue));
            }
            else
            {
                var generic = _listType.GetGenericArguments();
                if (generic.Length > 1) throw new InvalidOperationException();

                if (generic.Length == 0) _genericTemp = _listType;

                if (_genericTemp == null && generic[0] == typeof(TValue)) _genericTemp = _listType;
                else _genericTemp = _listType.GetGenericTypeDefinition().MakeGenericType(typeof(TValue));
            }

            if (_genericTemp == null) throw new InvalidOperationException();

            return Activator.CreateInstance(_genericTemp) ?? throw new InvalidOperationException("List Creation Failed");
        }

        public void Add(TKey key)
        {
            Argument.NotNull(key, nameof(key));

            if (!ContainsKey(key)) base[key] = (ICollection<TValue>) CreateList();
        }

        public void Add(TKey key, TValue value)
        {
            Argument.NotNull(key, nameof(key));
            Argument.NotNull(value, nameof(value));

            if (!ContainsKey(key)) Add(key);

            var list = base[key];
            list?.Add(value);
        }

        public void AddRange(TKey key, IEnumerable<TValue> value)
        {
            Argument.NotNull(key, nameof(key));
            // ReSharper disable once PossibleMultipleEnumeration
            Argument.NotNull(value, nameof(value));

            if (!ContainsKey(key)) Add(key);

            var values = base[key];
            if (values == null) return;
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var item in value.Where(item => item != null)) values.Add(item);
        }


        public bool RemoveValue(TValue value)
        {
            return RemoveImpl(default!, value, false, true);
        }

        public bool Remove(TValue value, bool removeEmptyLists)
        {
            return RemoveImpl(default!, value, removeEmptyLists, true);
        }

        public bool Remove(TKey key, TValue value)
        {
            return RemoveImpl(key, value, false, false);
        }

        public bool Remove(TKey key, TValue value, bool removeListIfEmpty)
        {
            return RemoveImpl(key, value, removeListIfEmpty, false);
        }

        private bool RemoveImpl(TKey key, TValue val, bool removeEmpty, bool removeAll)
        {
            var ok = false;

            if (removeAll)
            {
                var keys = Keys.ToArray().GetEnumerator();
                var vals = Values.ToArray().GetEnumerator();
                while (keys.MoveNext() && vals.MoveNext())
                {
                    var coll = vals.Current as ICollection<TValue> ?? Array.Empty<TValue>();
                    if (keys.Current is not TKey currkey)
                        throw new InvalidCastException();

                    ok |= RemoveList(coll, val);

                    // ReSharper disable once PossibleNullReferenceException
                    // ReSharper disable once AssignNullToNotNullAttribute
                    if (removeEmpty && coll.Count == 0) ok |= Remove(currkey);
                }
            }
            else
            {
                Argument.NotNull(key, nameof(key));

                ok = ContainsKey(key);
                if (!ok) return false;
                var col = base[key];

                ok |= RemoveList(col, val);
                if (!removeEmpty) return true;
                if (col.Count == 0) ok |= Remove(key);
            }

            return ok;
        }

        private static bool RemoveList(ICollection<TValue> vals, TValue val)
        {
            var ok = false;
            while (vals.Remove(val)) ok = true;

            return ok;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("listType", _listType, typeof(Type));

            base.GetObjectData(info, context);
        }

        private class AllValueCollection : ICollection<TValue>
        {
            private readonly GroupDictionary<TKey, TValue> _list;

            public AllValueCollection(GroupDictionary<TKey, TValue> list)
            {
                _list = Argument.NotNull(list, nameof(list));
            }

            private IEnumerable<TValue> GetAll => _list.SelectMany(pair => pair.Value);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public int Count => GetAll.Count();

            public bool IsReadOnly => true;

            public void Add(TValue item) => throw new NotSupportedException();

            public void Clear() => throw new NotSupportedException();

            public bool Contains(TValue item) => GetAll.Contains(item);

            public void CopyTo(TValue[] array, int arrayIndex) => GetAll.ToArray().CopyTo(array, arrayIndex);

            public IEnumerator<TValue> GetEnumerator() => GetAll.GetEnumerator();

            public bool Remove(TValue item) => throw new NotSupportedException();
        }
    }
}