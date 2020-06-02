using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Threading;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf
{
    //[DebuggerNonUserCode]
    [PublicAPI]
    [Serializable]
    public class UIObservableCollection<TType> : ObservableCollection<TType>
    {
        private bool _isBlocked;

        public UIObservableCollection()
        {
        }

        public UIObservableCollection(IEnumerable<TType> enumerable)
            : base(enumerable)
        {
        }

        [NotNull] protected Dispatcher InternalUISynchronize { get; } = System.Windows.Application.Current.Dispatcher;

        public void AddRange(IEnumerable<TType> enumerable)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
            foreach (var item in enumerable) Add(item);
        }

        public IDisposable BlockChangedMessages()
        {
            return new DispoableBlocker(this);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_isBlocked) return;
            if (InternalUISynchronize.CheckAccess())
                base.OnCollectionChanged(e);
            InternalUISynchronize.Invoke(() => base.OnCollectionChanged(e));
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (_isBlocked) return;
            if (InternalUISynchronize.CheckAccess()) base.OnPropertyChanged(e);
            else InternalUISynchronize.Invoke(() => base.OnPropertyChanged(e));
        }

        private class DispoableBlocker : IDisposable
        {
            private readonly UIObservableCollection<TType> _collection;

            public DispoableBlocker(UIObservableCollection<TType> collection)
            {
                _collection = collection;
                _collection._isBlocked = true;
            }

            public void Dispose()
            {
                _collection._isBlocked = false;
                _collection.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
    }
}