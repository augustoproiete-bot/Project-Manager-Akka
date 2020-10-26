using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CacheManager.Core;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Cache;
using Tauron.Application.Workshop.StateManagement.Dispatcher;
using Tauron.Application.Workshop.StateManagement.Internal;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    public abstract class StateBuilderBase
    {
        public abstract (StateContainer State, string Key) Materialize(WorkspaceSuperviser superviser, IStateDispatcherConfigurator config, ICache<object?>? parent);
    }

    public sealed class StateBuilder<TData> : StateBuilderBase, IStateBuilder<TData> 
        where TData : class
    {
        private readonly Func<IStateDataSource<TData>> _source;
        private readonly List<Func<IReducer<TData>>> _reducers = new List<Func<IReducer<TData>>>();

        private IState<TData>? _state;
        private bool _parentCache;
        private Action<ConfigurationBuilderCachePart>? _cacheConfigurator;
        private string? _key;

        public StateBuilder(Func<IStateDataSource<TData>> source) 
            => _source = source;

        public IStateBuilder<TData> WithStateType<TState>() 
            where TState : IState<TData>, new()
        {
            _state = new TState();
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

        public override (StateContainer State, string Key) Materialize(WorkspaceSuperviser superviser, IStateDispatcherConfigurator config, ICache<object?>? parent)
        {
            if(_state == null)
                throw new InvalidOperationException("A State type or Instance Must be set");

            ICache<TData>? cache = null;
            if (_parentCache && parent != null || _cacheConfigurator != null)
            {
                cache = CacheFactory.Build<TData>(_key ?? Guid.NewGuid().ToString("N"), c =>
                {
                    if (_parentCache && parent != null)
                        c.WithParentCache(ParentCache.Create(parent, false, o => o as TData, data => data));
                });
            }

            var container = new StateContainer<TData>(_state, _reducers.Select(r => r()).ToImmutableList(), cache, _source());

            _state.Initialize(new MutatingEngine<TData>());

            return (container, _key ?? string.Empty);
        }
    }
}