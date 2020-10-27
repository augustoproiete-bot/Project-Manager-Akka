using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CacheManager.Core;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Builder;
using Tauron.Application.Workshop.StateManagement.Cache;
using Tauron.Application.Workshop.StateManagement.Dispatcher;
using Tauron.Application.Workshop.StateManagement.Internal;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public sealed class RootManager : DisposeableBase, IActionInvoker
    {
        private readonly ConcurrentDictionary<string, ConcurrentBag<StateContainer>> _stateContainers = new ConcurrentDictionary<string, ConcurrentBag<StateContainer>>();
        private readonly StateContainer[] _states;
        private readonly MutatingEngine _engine;
        private readonly IEffect[] _effects;
        private readonly IMiddleware[] _middlewares;

        internal RootManager(WorkspaceSuperviser superviser, IStateDispatcherConfigurator stateDispatcher, IEnumerable<StateBuilderBase> states, 
            IEnumerable<IEffect?> effects, IEnumerable<IMiddleware?> middlewares, Action<ConfigurationBuilderCachePart>? globalCache)
        {
            _engine = MutatingEngine.Create(superviser, stateDispatcher.Configurate);
            _effects = effects.Where(e => e != null).ToArray()!;
            _middlewares = middlewares.Where(m => m != null).ToArray()!;

            ICache<object?>? cache = null;
            if(globalCache != null)
                cache = new SyncCache<object?>(CacheFactory.Build(globalCache));

            foreach (var stateBuilder in states)
            {
                var (container, key) = stateBuilder.Materialize(_engine, cache);
                _stateContainers.GetOrAdd(key, _ => new ConcurrentBag<StateContainer>()).Add(container);
            }

            _states = _stateContainers.SelectMany(b => b.Value).ToArray();

            foreach (var middleware in _middlewares) 
                middleware.Initialize(this);

            foreach (var middleware in _middlewares) 
                middleware.AfterInitializeAllMiddlewares();
        }

        public TState? GetState<TState>()
            where TState : class
            => GetState<TState>("");

        public TState? GetState<TState>(string key)
            where TState : class
        {
            if (_stateContainers.TryGetValue(key, out var bag))
            {
                foreach (var stateContainer in bag)
                {
                    if (stateContainer.Instance is TState state)
                        return state;
                }
            }

            return null;
        }

        public void Run(IStateAction action)
        {
            if(_middlewares.Any(m => !m.MayDispatchAction(action)))
                return;

            _middlewares.Foreach(m => m.BeforeDispatch(action));

            foreach (var dataMutation in _states.Select(sc => sc.TryDipatch(action)))
            {
                if(dataMutation != null)
                    _engine.Mutate(dataMutation);
            }

            _engine.Mutate(new EffectInvoker(_effects.Where(e => e.ShouldReactToAction(action)), action, this));
        }

        protected override void DisposeCore(bool disposing)
        {
            if (!disposing) return;
            
            _stateContainers.Values.Foreach(s => s.Foreach(d => d.Dispose()));
            _stateContainers.Clear();
        }

        private sealed class EffectInvoker : IDataMutation
        {
            private readonly IEnumerable<IEffect> _effects;
            private readonly IStateAction _action;
            private readonly IActionInvoker _invoker;

            public object ConsistentHashKey => "Effects";
            public string Name => "Invoke Effects";
            public Action Run => () => _effects.Foreach(e => e.Handle(_action, _invoker));

            public EffectInvoker(IEnumerable<IEffect> effects, IStateAction action, IActionInvoker invoker)
            {
                _effects = effects;
                _action = action;
                _invoker = invoker;
            }
        }
    }
}