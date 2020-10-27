using System;
using System.Threading;
using CacheManager.Core;

namespace Tauron.Application.Workshop.StateManagement.Cache
{
    public sealed class SyncCache<TData> : DisposeableBase, ICache<TData>
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly ICache<TData> _cache;

        public SyncCache(ICache<TData> cache) => _cache = cache;

        private IDisposable EnterRead()
        {
            _lock.EnterReadLock();
            return new ActionDispose(() => _lock.ExitReadLock());
        }

        private IDisposable EnterWrite()
        {
            _lock.EnterWriteLock();
            return new ActionDispose(() => _lock.ExitWriteLock());
        }

        public bool Add(string key, TData value)
        {
            using (EnterWrite())
                return _cache.Add(key, value);
        }

        public bool Add(string key, TData value, string region)
        {
            using (EnterWrite())
                return _cache.Add(key, value, region);
        }

        public bool Add(CacheItem<TData> item)
        {
            using (EnterWrite())
                return _cache.Add(item);
        }

        public void Clear()
        {
            using (EnterWrite())
                _cache.Clear();
        }

        public void ClearRegion(string region)
        {
            using (EnterWrite())
                _cache.ClearRegion(region);
        }

        public bool Exists(string key)
        {
            using (EnterRead())
                return _cache.Exists(key);
        }

        public bool Exists(string key, string region)
        {
            using (EnterRead())
                return _cache.Exists(key, region);
        }

        public TData Get(string key)
        {
            using (EnterRead())
                return _cache.Get(key);
        }

        public TData Get(string key, string region)
        {
            using (EnterRead())
                return _cache.Get(key, region);
        }

        public TOut Get<TOut>(string key)
        {
            return _cache.Get<TOut>(key);
        }

        public TOut Get<TOut>(string key, string region)
        {
            using (EnterRead())
                return _cache.Get<TOut>(key, region);
        }

        public CacheItem<TData> GetCacheItem(string key)
        {
            using (EnterRead())
                return _cache.GetCacheItem(key);
        }

        public CacheItem<TData> GetCacheItem(string key, string region)
        {
            using (EnterRead())
                return _cache.GetCacheItem(key, region);
        }

        public void Put(string key, TData value)
        {
            using (EnterWrite())
                _cache.Put(key, value);
        }

        public void Put(string key, TData value, string region)
        {
            using (EnterWrite())
                _cache.Put(key, value, region);
        }

        public void Put(CacheItem<TData> item)
        {
            using (EnterWrite())
                _cache.Put(item);
        }

        public bool Remove(string key)
        {
            using (EnterWrite())
                return _cache.Remove(key);
        }

        public bool Remove(string key, string region)
        {
            using (EnterWrite())
                return _cache.Remove(key, region);
        }

        public TData this[string key]
        {
            get
            {
                using (EnterRead()) return _cache[key];
            }
            set
            {
                using (EnterWrite()) _cache[key] = value;
            }
        }

        public TData this[string key, string region]
        {
            get
            {
                using (EnterRead()) return _cache[key, region];
            }
            set
            {
                using (EnterWrite()) _cache[key, region] = value;
            }
        }

        protected override void DisposeCore(bool disposing)
        {
            if (!disposing) return;

            _lock.Dispose();
            _cache.Dispose();
        }
    }
}