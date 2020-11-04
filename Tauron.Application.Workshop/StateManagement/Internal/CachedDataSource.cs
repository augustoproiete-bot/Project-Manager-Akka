using System;
using System.Threading.Tasks;
using CacheManager.Core;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Cache;

namespace Tauron.Application.Workshop.StateManagement.Internal
{
    public sealed class CachedDataSource<TData> : IExtendedDataSource<MutatingContext<TData>>, IDisposable
        where TData : class, IStateEntity
    {
        private readonly string _cacheKey;
        private readonly ICache<TData>? _cache;
        private readonly IExtendedDataSource<TData> _original;

        public CachedDataSource(string cacheKey, ICache<TData>? cache, IExtendedDataSource<TData> original)
        {
            _cacheKey = cacheKey;
            if (cache != null)
                _cache = new SyncCache<TData>(cache);
            _original = original;
        }

        public async Task<MutatingContext<TData>> GetData(IQuery query)
        {
            return MutatingContext<TData>.New(_cache?.Get(query.ToHash(), _cacheKey) ?? await _original.GetData(query));
        }

        public async Task SetData(IQuery query, MutatingContext<TData> data)
        {
            var (_, entity) = data;

            if (entity is IChangeTrackable trackable && !trackable.IsChanged) return;

            if (!(query is INoCache || entity is INoCache))
            {
                if (entity.IsDeleted)
                    _cache?.Remove(entity.Id, _cacheKey);
                else
                    _cache?.Add(entity.Id, entity, _cacheKey);
            }

            await _original.SetData(query, entity);
        }
        
        public void Dispose() => _cache?.Dispose();
    }
}