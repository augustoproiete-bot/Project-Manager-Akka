using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Autofac;
using Functional.Maybe;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Internal;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    public abstract class StateBuilderBase
    {
        public abstract (StateContainer State, Maybe<string> Key) Materialize(MutatingEngine engine, Maybe<IComponentContext> mayComponentContext);
    }

    public sealed class StateBuilder<TData> : StateBuilderBase, IStateBuilder<TData>
        where TData : class
    {
        private readonly Func<IExtendedDataSource<TData>> _source;
        private readonly List<Func<IReducer<TData>>> _reducers = new List<Func<IReducer<TData>>>();

        private Maybe<Type> _state;
        private Maybe<string> _key;

        public StateBuilder(Func<IExtendedDataSource<TData>> source)
            => _source = source;

        public IStateBuilder<TData> WithStateType<TState>()
            where TState : IState<TData>
        {
            _state = typeof(TState).ToMaybe();
            return this;
        }

        public IStateBuilder<TData> WithStateType(Type type)
        {
            _state = type.ToMaybe();
            return this;
        }

        public IStateBuilder<TData> WithReducer(Func<IReducer<TData>> reducer)
        {
            _reducers.Add(reducer);
            return this;
        }

        public IStateBuilder<TData> WithKey(string key)
        {
            _key = key.ToMaybe();
            return this;
        }

        public override (StateContainer State, Maybe<string> Key) Materialize(MutatingEngine engine, Maybe<IComponentContext> mayComponentContext)
        {
            var dataSource = new ContextDataSource<TData>(_source());
            var dataEngine = MutatingEngine.From(dataSource, engine);
            var mayState =
            (
                from componentContext in mayComponentContext
                from state in _state
                select Maybe.NotNull(componentContext.ResolveOptional(state, new TypedParameter(dataEngine.GetType(), dataEngine)) as IState)
            ).Collapse();

            mayState = mayState.Or(() =>
            (
                from state in _state
                select Maybe.NotNull(FastReflection.Shared.FastCreateInstance(state, dataEngine) as IState)
            ).Collapse());

            switch (mayState.OrElse(() => new InvalidOperationException("A State type or Instance Must be set")))
            {
                case ICanQuery<TData> canQuery:
                    canQuery.DataSource(dataSource);
                    break;
                case null:
                    throw new InvalidOperationException("Failed to Create State");
            }

            var container = new StateContainer<TData>(mayState, _reducers.Select(r => r()).ToImmutableList(), dataEngine, dataSource);

            return (container, _key);
        }
    }


}