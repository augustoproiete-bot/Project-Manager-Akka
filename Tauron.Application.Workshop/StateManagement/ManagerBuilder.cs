using System;
using System.Collections.Generic;
using CacheManager.Core;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Builder;
using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public sealed class ManagerBuilder
    {
        public static RootManager CreateManager(WorkspaceSuperviser superviser, Action<ManagerBuilder> builder)
        {
            var managerBuilder = new ManagerBuilder(superviser);
            builder(managerBuilder);
            return managerBuilder.Build();
        }

        private readonly WorkspaceSuperviser _superviser;

        private Func<IStateDispatcherConfigurator> _dispatcherFunc = () => new DefaultStateDispatcher();
        private List<Func<IEffect>> _effects = new List<Func<IEffect>>();
        private List<Func<IMiddleware>> _middlewares = new List<Func<IMiddleware>>();
        private List<StateBuilderBase> _states = new List<StateBuilderBase>();
        private Func<Action<ConfigurationBuilderCachePart>>? _globalCache;

        private ManagerBuilder(WorkspaceSuperviser superviser) 
            => _superviser = superviser;

        public IStateBuilder<TData> WithDataSource<TData>(Func<IStateDataSource<TData>> source) 
            where TData : class
        {
            var builder = new StateBuilder<TData>(source);
            _states.Add(builder);
            return builder;
        }

        public ManagerBuilder WithMiddleware(Func<IMiddleware> middleware)
        {
            _middlewares.Add(middleware);
            return this;
        }

        public ManagerBuilder WithEffect(Func<IEffect> effect)
        {
            _effects.Add(effect);
            return this;
        }

        public ManagerBuilder WithDispatcher(Func<IStateDispatcherConfigurator> factory)
        {
            _dispatcherFunc = factory;
            return this;
        }

        internal RootManager Build() 
            => new RootManager(_superviser, _dispatcherFunc());
    }
}