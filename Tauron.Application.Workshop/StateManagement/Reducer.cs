using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutating;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public abstract class Reducer<TAction, TData> : IReducer<TData>
        where TData : IStateEntity
        where TAction : IStateAction
    {
        public virtual IValidator<TAction>? Validator { get; }

        public virtual ReducerResult<TData> Reduce(MutatingContext<TData> state, IStateAction action)
        {
            try
            {
                var typedAction = (TAction) action;

                if (Validator == null) return Reduce(state, (TAction) action);
                var result = Validator.Validate(typedAction);
            
                return !result.IsValid ? ReducerResult.Fail(state, result.Errors.Select(f => f.ErrorMessage)) : Reduce(state, (TAction) action);
            }
            catch (Exception e)
            {
                return ReducerResult.Fail(state, new[] {e.Message});
            }
        }

        protected abstract ReducerResult<TData> Reduce(MutatingContext<TData> state, TAction action);

        protected ReducerResult<TData> Sucess(MutatingContext<TData> data)
            => ReducerResult.Sucess(data);

        protected ReducerResult<TData> Fail(MutatingContext<TData> data, IEnumerable<string> errors)
            => ReducerResult.Fail(data, errors);

        public virtual bool ShouldReduceStateForAction(IStateAction action) => action is TAction;
    }
}