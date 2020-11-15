using System;
using Functional.Either;
using JetBrains.Annotations;

namespace Tauron
{
    [PublicAPI]
    public static class EitherExtensions
    {
        public static TResult GetOrThrow<TResult>(this Either<TResult, Exception> either) 
            => either.Match(r => r, e => throw e);

        public static void OnError<TResult, TError>(this Either<TResult, TError> either, Action<TError> err) 
            => either.Match(_ =>{}, err);
    }
}