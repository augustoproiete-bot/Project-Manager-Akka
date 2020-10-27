using System;
using System.Collections.Generic;
using System.Linq;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.Internal
{
    public abstract class StateContainer : IDisposable
    {
        public IState Instance { get; }

        protected StateContainer(IState instance) 
            => Instance = instance;

        public abstract IDataMutation? TryDipatch(IStateAction action);
        public abstract void Dispose();
    }

    public sealed class StateContainer<TData> : StateContainer
        where TData : class
    {
        private readonly IDisposable _toDispose;
        private IReadOnlyCollection<IReducer<TData>> Reducers { get; }
        private MutatingEngine<MutatingContext<TData>> MutatingEngine { get; }

        public StateContainer(IState instance, IReadOnlyCollection<IReducer<TData>> reducers, MutatingEngine<MutatingContext<TData>> mutatingEngine, IDisposable toDispose)
            : base(instance)
        {
            _toDispose = toDispose;
            Reducers = reducers;
            MutatingEngine = mutatingEngine;
        }

        public override IDataMutation? TryDipatch(IStateAction action)
        {
            var reducers = Reducers.Where(r => r.ShouldReduceStateForAction(action)).ToList();
            return reducers.Count == 0 ? null : MutatingEngine.CreateMutate(action.ActionName, data => Reducers.Aggregate(data, (input, reducer) => reducer.Reduce(input, action)), action.Query);
        }

        public override void Dispose() 
            => _toDispose.Dispose();
    }
}