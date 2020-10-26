using System;
using CacheManager.Core;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.Cache
{
    public static class ParentCache
    {
        [PublicAPI]
        public static ParentCache<TValue> Create<TValue, TParent>(ICache<TParent> parent, bool disposeParent, Func<TParent, TValue> convertback, Func<TValue, TParent> convert)
            => new ParentCache<TValue>.ParentCacheImpl<TParent>(parent, disposeParent, convert, convertback);
    }

    public abstract class ParentCache<TValue> : IDisposable
    {
        public abstract bool Exists(string key, string prefix);

        public abstract void ClearRegion(string region);

        public abstract bool Exists(string key);

        public abstract CacheItem<TValue>? GetCacheItem(string key);

        public abstract CacheItem<TValue>? GetCacheItem(string key, string prefix);
        public abstract bool Remove(string key, string prefix);
        public abstract bool Add(CacheItem<TValue> cacheItem);
        public abstract void Put(CacheItem<TValue> cacheItem);
        public abstract void Dispose();

        internal sealed class ParentCacheImpl<TParent> : ParentCache<TValue>
        {
            private readonly ICache<TParent> _parent;
            private readonly bool _disposeParent;
            private readonly Func<TValue, TParent> _convert;
            private readonly Func<TParent, TValue> _convertback;

            public ParentCacheImpl(ICache<TParent> parent, bool disposeParent, Func<TValue, TParent> convert, Func<TParent, TValue> convertback)
            {
                _parent = parent;
                _disposeParent = disposeParent;
                _convert = convert;
                _convertback = convertback;
            }

            public override bool Exists(string key, string prefix) => _parent.Exists(key, prefix);

            public override void ClearRegion(string region) => _parent.ClearRegion(region);

            public override bool Exists(string key) => _parent.Exists(key);

            public override CacheItem<TValue>? GetCacheItem(string key) => ConvertBack(_parent.GetCacheItem(key));

            public override CacheItem<TValue>? GetCacheItem(string key, string prefix) => ConvertBack(_parent.GetCacheItem(key, prefix));

            public override bool Remove(string key, string prefix) => _parent.Remove(key, prefix);

            public override bool Add(CacheItem<TValue> cacheItem) => _parent.Add(Convert(cacheItem));

            public override void Put(CacheItem<TValue> cacheItem) => _parent.Put(Convert(cacheItem));

            public override void Dispose()
            {
                if(_disposeParent)
                    _parent.Dispose();
            }

            private CacheItem<TParent>? Convert(CacheItem<TValue>? parent) 
                => parent == null ? null : new CacheItem<TParent>(parent.Key, parent.Region ?? "null", _convert(parent.Value), parent.ExpirationMode, parent.ExpirationTimeout);

            private CacheItem<TValue>? ConvertBack(CacheItem<TParent>? parent)
                => parent == null ? null : new CacheItem<TValue>(parent.Key, parent.Region ?? "null", _convertback(parent.Value), parent.ExpirationMode, parent.ExpirationTimeout);
        }
    }
}