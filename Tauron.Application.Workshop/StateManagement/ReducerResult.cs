using System.Collections.Generic;
using System.Linq;
using Tauron.Application.Workshop.Mutating;

namespace Tauron.Application.Workshop.StateManagement
{
    public static class ReducerResult
    {
        public static ReducerResult<TData> Sucess<TData>(MutatingContext<TData> data)
            => new ReducerResult<TData>(data, null);

        public static ReducerResult<TData> Fail<TData>(MutatingContext<TData> data, IEnumerable<string> errors)
        {
            if(errors is string[] array)
                return new ReducerResult<TData>(data, array);

            return new ReducerResult<TData>(data, errors.ToArray());
        }
    }

    public interface IReducerResult
    {
        bool IsOk { get; }
        string[]? Errors { get; }
    }

    public sealed class ReducerResult<TData> : IReducerResult
    {
        public MutatingContext<TData> Data { get; }

        public string[]? Errors { get; }

        public bool IsOk => Errors == null;


        internal ReducerResult(MutatingContext<TData> data, string[]? errors)
        {
            Data = data;
            Errors = errors;
        }
    }
}