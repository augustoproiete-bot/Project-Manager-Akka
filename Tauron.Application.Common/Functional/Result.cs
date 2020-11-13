using System;
using Functional.Either;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Operations;

namespace Tauron
{
    [PublicAPI]
    public static class Result
    {
        public static Either<Maybe<TType>, Error[]> FromOperation<TType>(this IOperationResult operationResult)
        {
            if (!operationResult.Ok)
                return operationResult.Errors ?? Array.Empty<Error>();
            if (operationResult.Outcome is TType outcome)
                return outcome.ToMaybe();
            return Maybe<TType>.Nothing;
        }
    }
}