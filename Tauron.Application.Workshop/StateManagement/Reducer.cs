using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutating;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public abstract class Reducer<TAction, TData> : IReducer<TData>
        where TAction : IStateAction
    {
        public virtual IValidator<TAction>? Validator { get; }

        public virtual async Task<Maybe<ReducerResult<TData>>> Reduce(Maybe<MutatingContext<TData>> state, IStateAction action)
        {
            try
            {
                var typedAction = (TAction) action;

                if (Validator == null) return await Reduce(state, (TAction) action);
                var result = await Validator.ValidateAsync(typedAction);
            
                return !result.IsValid ? ReducerResult.Fail(state, result.Errors.Select(f => f.ErrorMessage)) : await Reduce(state, (TAction) action);
            }
            catch (Exception e)
            {
                return ReducerResult.Fail(state, new[] {e.Message});
            }
        }

        protected abstract Task<Maybe<ReducerResult<TData>>> Reduce(Maybe<MutatingContext<TData>> state, TAction action);

        protected Maybe<ReducerResult<TData>> Sucess(Maybe<MutatingContext<TData>> data)
            => ReducerResult.Sucess(data);

        protected Maybe<ReducerResult<TData>> Fail(Maybe<MutatingContext<TData>> data, IEnumerable<string> errors)
            => ReducerResult.Fail(data, errors);

        protected Task<Maybe<ReducerResult<TData>>> SucessAsync(Maybe<MutatingContext<TData>> data)
            => Task.FromResult(ReducerResult.Sucess(data));

        protected Task<Maybe<ReducerResult<TData>>> FailAsync(Maybe<MutatingContext<TData>> data, IEnumerable<string> errors)
            => Task.FromResult(ReducerResult.Fail(data, errors));

        public virtual bool ShouldReduceStateForAction(IStateAction action) => action is TAction;
    }
}