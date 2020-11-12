using System;
using JetBrains.Annotations;
using Tauron.Operations;

namespace AkkaTest.Test
{
    [PublicAPI]
    public interface IResult<TType> : IMaybe<TType>
    {
        IOperationResult Underlaying { get; }

        Maybe<TResult> Process<TResult>(Func<TType, TResult> process);

        bool Success { get; }
    }

    [PublicAPI]
    public readonly struct Result<TType> : IResult<TType>
    {
        private readonly Maybe<TType> _value;

        public Result(IOperationResult result)
        {
            Underlaying = result;
            _value = Maybe.From<TType>(() => result.Ok && result is {Outcome: TType value} ? value : default);
        }

        public Result(Maybe<TType> maybe)
        {
            _value = maybe;
            Underlaying = null;
        }

        public IOperationResult? Underlaying { get; }

        public Maybe<TResult> Process<TResult>(Func<TType, TResult> process) 
            => _value.Map(process);

        public bool Success => Underlaying?.Ok ?? _value.HasValue;

        bool IMaybe<TType>.HasValue => _value.HasValue;

        public TType? Value => _value.Value;

        void IMaybe<TType>.OnValue(Action<TType> valueAction) => _value.OnValue(valueAction);

        void IMaybe<TType>.OnNothing(Action action) => _value.OnNothing(action);

        //Nullable dont works on Explicit Implementation
        #pragma warning disable CS8617 // Die NULL-Zulässigkeit von Verweistypen im Typ des Parameters entspricht nicht dem implementierten Member.
        Maybe<TResult> IMaybe<TType>.Map<TResult>(Func<TType, TResult> map) => _value.Map(map)!;
        #pragma warning restore CS8617 // Die NULL-Zulässigkeit von Verweistypen im Typ des Parameters entspricht nicht dem implementierten Member.

        Maybe<TResult> IMaybe<TType>.Bind<TResult>(Func<TType, Maybe<TResult>> map) => _value.Bind(map);

        Maybe<TType> IMaybe<TType>.AsNothing() => _value.AsNothing();

        Maybe<TType> IMaybe<TType>.AsMaybe() => _value.AsMaybe();
    }

    [PublicAPI]
    public static class Result
    {
        public static IResult<TType> FromOperation<TType>(IOperationResult operationResult)
            => new Result<TType>(operationResult);

        public static IResult<TType> Create<TType>(TType value)
            => new Result<TType>(OperationResult.Success(value));
    }

    [PublicAPI]
    public static class ResultExtensions
    {
        public static IResult<TResult> Catch<TType, TResult>(this IMaybe<TType> maybe, Func<TType, TResult?> map)
        {
            try
            {
                return new Result<TResult>(maybe.Map(map)!);
            }
            catch (Exception e)
            {
                return new Result<TResult>(OperationResult.Failure(e));
            }
        }
    }
}