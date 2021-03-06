﻿using System;
using System.Collections.Generic;
using System.Linq;
using Functional.Maybe;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using static Tauron.Prelude;

namespace Tauron.Application.Workshop.StateManagement.Internal
{
    public abstract class StateContainer : IDisposable
    {
        public Maybe<IState> Instance { get; }

        protected StateContainer(Maybe<IState> instance) 
            => Instance = instance;

        public abstract IDataMutation? TryDipatch(IStateAction action, Action<IReducerResult> sendResult, Action onCompled);
        public abstract void Dispose();
    }

    public sealed class StateContainer<TData> : StateContainer
        where TData : class
    {
        private readonly IDisposable _toDispose;
        private IReadOnlyCollection<IReducer<TData>> Reducers { get; }
        private ExtendedMutatingEngine<MutatingContext<TData>> MutatingEngine { get; }

        public StateContainer(Maybe<IState> instance, IReadOnlyCollection<IReducer<TData>> reducers, ExtendedMutatingEngine<MutatingContext<TData>> mutatingEngine, IDisposable toDispose)
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

            return MutatingEngine.CreateMutate(action.ActionName, action.Query,
                                               async data =>
                                               {
                                                   try
                                                   {
                                                       var isFail = false;
                                                       foreach (var reducer in reducers)
                                                       {
                                                           var mayResult = await reducer.Reduce(data, action);

                                                           var tempData =
                                                               Collapse(from result in mayResult
                                                                        from _ in MayUse(() => sendResult(result))
                                                                        where result.IsOk
                                                                        select result.Data);

                                                           if (tempData.IsNothing())
                                                               isFail = true;

                                                           data = tempData.Or(data);
                                                       }

                                                       return isFail ? Maybe<MutatingContext<TData>>.Nothing : data;
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