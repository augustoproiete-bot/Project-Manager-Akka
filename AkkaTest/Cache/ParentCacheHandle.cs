using System;
using System.Collections.Generic;
using CacheManager.Core;
using CacheManager.Core.Internal;
using CacheManager.Core.Logging;

namespace AkkaTest.Cache
{

    public sealed class ParentCacheHandle<TCacheValue> : BaseCacheHandle<TCacheValue>
    {
        private readonly ParentCache<TCacheValue> _parent;
        private readonly HashSet<string> _regions = new HashSet<string>();
        private readonly string _prefix = Guid.NewGuid().ToString("N");

        public ParentCacheHandle(
            ICacheManagerConfiguration managerConfiguration,
            CacheHandleConfiguration configuration,
            ILoggerFactory loggerFactory, 
            ParentCache<TCacheValue> parentCache)
            : base(managerConfiguration, configuration)
        {
            _parent = parentCache;
            Logger = loggerFactory.CreateLogger(this);
        }

        public override void Clear()
        {
            foreach (var region in _regions)
                _parent.ClearRegion(_prefix + region);
            _parent.ClearRegion(_prefix);
            _regions.Clear();
        }

        public override void ClearRegion(string region)
        {
            if (_regions.Remove(region))
                _parent.ClearRegion(_prefix + region);
        }

        public override bool Exists(string key)
            => _parent.Exists(key, _prefix) || _parent.Exists(key);

        public override bool Exists(string key, string region)
            => _parent.Exists(key, _prefix + region) || _parent.Exists(key, region);

        protected override CacheItem<TCacheValue> GetCacheItemInternal(string key)
            => _parent.GetCacheItem(key, _prefix) ?? _parent.GetCacheItem(key)!;

        protected override CacheItem<TCacheValue> GetCacheItemInternal(string key, string region)
            => _parent.GetCacheItem(key, _prefix + region) ?? _parent.GetCacheItem(key, region)!;

        protected override bool RemoveInternal(string key) => _parent.Remove(key, _prefix);

        protected override bool RemoveInternal(string key, string region)
            => _parent.Remove(key, _prefix + region);

        protected override ILogger Logger { get; }

        protected override bool AddInternalPrepared(CacheItem<TCacheValue> item)
        {
            if (!_parent.Add(new CacheItem<TCacheValue>(item.Key, _prefix + item.Region, item.Value, item.ExpirationMode, item.ExpirationTimeout))) return false;

            if (item.Region != null)
                _regions.Add(item.Region);

            return true;

        }

        protected override void PutInternalPrepared(CacheItem<TCacheValue> item)
        {
            _parent.Put(new CacheItem<TCacheValue>(item.Key, _prefix + item.Region, item.Value, item.ExpirationMode, item.ExpirationTimeout));

            if (item.Region != null)
                _regions.Add(item.Region);
        }

        public override int Count => 0;

        protected override void Dispose(bool disposeManaged)
        {
            if (disposeManaged)
                _parent.Dispose();
            base.Dispose(disposeManaged);
        }
    }
}