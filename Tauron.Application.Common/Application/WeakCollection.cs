using Functional.Maybe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Tauron.Application
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed class WeakCollection<TType> : IList<Maybe<TType>>
        where TType : class
    {
        private readonly List<WeakReference<TType>> _internalCollection = new();

        public WeakCollection() => WeakCleanUp.RegisterAction(CleanUp);

        public int EffectiveCount => _internalCollection.Count(refer => refer.IsAlive());

        public Maybe<TType> this[int index]
        {
            get => _internalCollection[index].TypedTarget();
            set => value.Do(v => _internalCollection[index] = new WeakReference<TType>(v));
        }
        IEnumerator IEnumerable.GetEnumerator() 
            => GetEnumerator();

        public int Count => _internalCollection.Count;

        public bool IsReadOnly => false;

        public void Add(Maybe<TType> item) 
            => item.Do(i => _internalCollection.Add(new WeakReference<TType>(i)));

        /// <summary>The clear.</summary>
        public void Clear() 
            => _internalCollection.Clear();

        public bool Contains(Maybe<TType> item)
        {
            var item1 = item.OrElseDefault();
            return _internalCollection.Any(it => it.TypedTarget().OrElseDefault() == item1);
        }

        public void CopyTo(Maybe<TType>[] array, int arrayIndex)
        {
            Argument.NotNull(array, nameof(array));

            for (var i = arrayIndex; i < array.Length; i++) 
                array[i] = _internalCollection[i].TypedTarget();
        }

        public IEnumerator<Maybe<TType>> GetEnumerator() 
            => _internalCollection.Select(reference => reference.TypedTarget()).GetEnumerator();

        public int IndexOf(Maybe<TType> item)
        {
            return (from realitem in item
                select Search(realitem)).OrElse(-1);

            int Search(TType it)
            {
                int index;
                for (index = 0; index < _internalCollection.Count; index++)
                {
                    var temp = _internalCollection[index];
                    if (temp.TypedTarget().OrElseDefault() == it) break;
                }

                return index == _internalCollection.Count ? -1 : index;
            }
        }

        public void Insert(int index, Maybe<TType> item) 
            => item.Do(realItem =>_internalCollection.Insert(index, new WeakReference<TType>(realItem)));

        public bool Remove(Maybe<TType> item)
        {
            var index = IndexOf(item);
            if (index == -1) return false;

            _internalCollection.RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index) 
            => _internalCollection.RemoveAt(index);

        public event EventHandler? CleanedEvent;

        internal void CleanUp()
        {
            var dead = _internalCollection.Where(reference => !reference.IsAlive()).ToArray();
            foreach (var genericWeakReference in dead) _internalCollection.Remove(genericWeakReference);

            OnCleaned();
        }

        private void OnCleaned() 
            => CleanedEvent?.Invoke(this, EventArgs.Empty);
    }

    [DebuggerNonUserCode, PublicAPI]
    public class WeakReferenceCollection<TType> : Collection<TType>
        where TType : IWeakReference
    {
        public WeakReferenceCollection()
        {
            WeakCleanUp.RegisterAction(CleanUpMethod);
        }

        protected override void ClearItems()
        {
            lock (this)
            {
                base.ClearItems();
            }
        }

        protected override void InsertItem(int index, TType item)
        {
            lock (this)
            {
                if (index > Count) index = Count;
                base.InsertItem(index, item);
            }
        }

        protected override void RemoveItem(int index)
        {
            lock (this)
            {
                base.RemoveItem(index);
            }
        }

        protected override void SetItem(int index, TType item)
        {
            lock (this)
            {
                base.SetItem(index, item);
            }
        }

        private void CleanUpMethod()
        {
            lock (this)
            {
                foreach (var weakReference in Items.ToArray()
                    .Where(it => !it.IsAlive)
                    .ToArray())
                {
                    if (weakReference is IDisposable dis) dis.Dispose();

                    Items.Remove(weakReference);

                }
            }
        }
    }
}