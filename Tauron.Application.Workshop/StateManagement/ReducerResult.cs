using System.Collections.Generic;
using System.Linq;
using Functional.Maybe;
using Tauron.Application.Workshop.Mutating;

namespace Tauron.Application.Workshop.StateManagement
{
    public static class ReducerResult
    {
        public static Maybe<ReducerResult<TData>> Sucess<TData>(Maybe<MutatingContext<TData>> data)
            => new ReducerResult<TData>(data, Maybe<string[]>.Nothing).ToMaybe();

        public static Maybe<ReducerResult<TData>> Fail<TData>(Maybe<MutatingContext<TData>> data, IEnumerable<string> errors)
        {
            if(errors is string[] array)
                return new ReducerResult<TData>(data, array.ToMaybe()).ToMaybe();

            return new ReducerResult<TData>(data, errors.ToArray().ToMaybe()).ToMaybe();
        }
    }

    public interface IReducerResult
    {
        bool IsOk { get; }
        Maybe<string[]> Errors { get; }
    }

    public sealed record ReducerResult<TData>(Maybe<MutatingContext<TData>> Data, Maybe<string[]> Errors) : IReducerResult
    {
        public bool IsOk => Errors.IsNothing();
    }
}