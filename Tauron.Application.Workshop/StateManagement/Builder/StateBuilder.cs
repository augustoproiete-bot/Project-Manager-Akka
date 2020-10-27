using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CacheManager.Core;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Cache;
using Tauron.Application.Workshop.StateManagement.Internal;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    public abstract class StateBuilderBase
    {
        public abstract (StateContainer State, string Key) Materialize(MutatingEngine engine, ICache<object?>? parent);
    }

    public sealed class StateBuilder<TData> : StateBuilderBase, IStateBuilder<TData> 
        where TData : class
    {
        private readonly Func<IStateDataSource<TData>> _source;
        private readonly List<Func<IReducer<TData>>> _reducers = new List<Func<IReducer<TData>>>();

        private Type? _state;
        private bool _parentCache;
        private Action<ConfigurationBuilderCachePart>? _cacheConfigurator;
        private string? _key;

        public StateBuilder(Func<IStateDataSource<TData>> source) 
            => _source = source;

        public IStateBuilder<TData> WithStateType<TState>() 
            where TState : IState<TData>
        {
            _state = typeof(TState);
            return this;
        }

        public IStateBuilder<TData> WithNoCache()
        {
            _parentCache = false;
            _cacheConfigurator = null;
            return this;
        }

        public IStateBuilder<TData> WithParentCache()
        {
            _parentCache = true;
            return this;
        }

        public IStateBuilder<TData> WithCache(Action<ConfigurationBuilderCachePart> cache)
        {
            _cacheConfigurator = cache;
            return this;
        }

        public IStateBuilder<TData> WithReducer(Func<IReducer<TData>> reducer)
        {
            _reducers.Add(reducer);
            return this;
        }

        public IStateBuilder<TData> WithKey(string key)
        {
            _key = key;
            return this;
        }

        public override (StateContainer State, string Key) Materialize(MutatingEngine engine, ICache<object?>? parent)
        {
            if (_state == null)
                throw new InvalidOperationException("A State type or Instance Must be set");

            ICache<TData>? cache = null;
            if (_parentCache && parent != null || _cacheConfigurator != null)
            {
                cache = CacheFactory.Build<TData>(_key ?? Guid.NewGuid().ToString("N"), c =>
                {
                    if (_parentCache && parent != null)
                        c.WithParentCache(ParentCache.Create(parent, false, o => o as TData, data => data));
                    _cacheConfigurator?.Invoke(c);
                });
            }

            var cacheKey = $"{_state.Name}--{Guid.NewGuid():N}";
            var dataSource = new CachedDataSource(cacheKey, cache, _source());


            var dataEngine = MutatingEngine.From(dataSource, engine);
            if(!(_state.FastCreateInstance(dataEngine) is IState state))
                throw new InvalidOperationException("Failed to Create State");

            var container = new StateContainer<TData>(state, _reducers.Select(r => r()).ToImmutableList(), dataEngine, dataSource);

            return (container, _key ?? string.Empty);
        }

        private sealed class CachedDataSource : IStateDataSource<MutatingContext<TData>>, IDisposable
        {
            private readonly string _cacheKey;
            private readonly ICache<TData>? _cache;
            private readonly IStateDataSource<TData> _original;
            private string _lastQuery = string.Empty;

            public CachedDataSource(string cacheKey, ICache<TData>? cache, IStateDataSource<TData> original)
            {
                _cacheKey = cacheKey;
                if(cache != null)
                    _cache = new SyncCache<TData>(cache);
                _original = original;
            }

            public MutatingContext<TData> GetData() 
                => new MutatingContext<TData>(null, _cache?.Get(_lastQuery, _cacheKey) ?? _original.GetData());

            public void SetData(MutatingContext<TData> data)
            {
                if (data.Data is IChangeTrackable trackable && !trackable.IsChanged)
                    return;

                _cache?.Put(_lastQuery, data.Data, _cacheKey);
                _original.SetData(data.Data);
            }

            public void Apply(string query)
            {
                _lastQuery = query;
                _original.Apply(query);
            }

            public void Dispose() => _cache?.Dispose();
        }
    }
}