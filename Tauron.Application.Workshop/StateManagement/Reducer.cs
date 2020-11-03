using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public virtual async Task<ReducerResult<TData>> Reduce(MutatingContext<TData> state, IStateAction action)
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

        protected abstract Task<ReducerResult<TData>> Reduce(MutatingContext<TData> state, TAction action);

        protected ReducerResult<TData> Sucess(MutatingContext<TData> data)
            => ReducerResult.Sucess(data);

        protected ReducerResult<TData> Fail(MutatingContext<TData> data, IEnumerable<string> errors)
            => ReducerResult.Fail(data, errors);

        protected Task<ReducerResult<TData>> SucessAsync(MutatingContext<TData> data)
            => Task.FromResult(ReducerResult.Sucess(data));

        protected Task<ReducerResult<TData>> FailAsync(MutatingContext<TData> data, IEnumerable<string> errors)
            => Task.FromResult(ReducerResult.Fail(data, errors));

        public virtual bool ShouldReduceStateForAction(IStateAction action) => action is TAction;
    }
}