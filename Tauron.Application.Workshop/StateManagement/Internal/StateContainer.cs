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

        public abstract IDataMutation? TryDipatch(IStateAction action, Action<IReducerResult> sendResult, Action onCompled);
        public abstract void Dispose();
    }

    public sealed class StateContainer<TData> : StateContainer
        where TData : class
    {
        private readonly IDisposable _toDispose;
        private IReadOnlyCollection<IReducer<TData>> Reducers { get; }
        private QueryableMutatingEngine<MutatingContext<TData>> MutatingEngine { get; }

        public StateContainer(IState instance, IReadOnlyCollection<IReducer<TData>> reducers, QueryableMutatingEngine<MutatingContext<TData>> mutatingEngine, IDisposable toDispose)
            : base(instance)
        {
            _toDispose = toDispose;
            Reducers = reducers;
            MutatingEngine = mutatingEngine;
        }

        public override IDataMutation? TryDipatch(IStateAction action, Action<IReducerResult> sendResult, Action onCompled)
        {
            var reducers = Reducers.Where(r => r.ShouldReduceStateForAction(action)).ToList();
            if (reducers.Count == 0)
                return null;

            return MutatingEngine.CreateMutate(action.ActionName, action.Query, async data =>
            {
                try
                {
                    var isFail = false;
                    foreach (var reducer in reducers)
                    {
                        var result = await reducer.Reduce(data, action);

                        if (!result.IsOk)
                            isFail = true;

                        sendResult(result);
                        data = result.Data;
                    }

                    return isFail ? null : data;
                }
                finally
                {
                    onCompled();
                }
            });
        }

        public override void Dispose() 
            => _toDispose.Dispose();
    }
}