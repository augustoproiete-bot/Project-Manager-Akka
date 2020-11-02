using System;
using CacheManager.Core;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.StateManagement.Cache;

namespace Tauron.Application.Workshop.StateManagement.Internal
{
    public sealed class CachedDataSource<TData> : IStateDataSource<MutatingContext<TData>>, IDisposable
        where TData : class, IStateEntity
    {
        private readonly string _cacheKey;
        private readonly ICache<TData>? _cache;
        private readonly IStateDataSource<TData> _original;
        private IQuery? _lastQuery;

        public CachedDataSource(string cacheKey, ICache<TData>? cache, IStateDataSource<TData> original)
        {
            _cacheKey = cacheKey;
            if (cache != null)
                _cache = new SyncCache<TData>(cache);
            _original = original;
        }

        public MutatingContext<TData> GetData()
        {
            if (_lastQuery == null)
                throw new InvalidOperationException("No Query was set");

            return MutatingContext<TData>.New(_cache?.Get(_lastQuery.ToHash(), _cacheKey) ?? _original.GetData());
        }

        public void SetData(MutatingContext<TData> data)
        {
            if (_lastQuery == null)
                throw new InvalidOperationException("No Query was set");

            var (_, entity) = data;

            if (entity is IChangeTrackable trackable && !trackable.IsChanged) return;

            if (!(_lastQuery is INoCache || entity is INoCache))
            {
                if (entity.IsDeleted)
                    _cache?.Remove(entity.Id, _cacheKey);
                else
                    _cache?.Add(entity.Id, entity, _cacheKey);
            }

            _original.SetData(entity);
        }

        public void Apply(IQuery query)
        {
            _lastQuery = query;
            _original.Apply(query);
        }

        public void Dispose() => _cache?.Dispose();
    }
}