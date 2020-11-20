using System;
using System.Collections.Generic;
using System.Linq;
using Functional.Maybe;
using JetBrains.Annotations;

namespace Tauron
{
    [PublicAPI]
    public static class F
    {
        public static Func<TInput, TOutput> Compose<TInput, TMiddle, TOutput>(this Func<TInput, TMiddle> input, Func<TMiddle, TOutput> output)
            => i => output(input(i));

        public static Func<TOutput> Compose<TMiddle, TOutput>(this Func<TMiddle> input, Func<TMiddle, TOutput> output)
            => () => output(input());

        public static Func<TOutput> CloseFunc<TInput, TOutput>(this Func<TInput, TOutput> output, Func<TInput> toClose)
            => () => output(toClose());
        
        public static Maybe<IEnumerable<TResult>> SelectMany<T, TTempResult, TResult>(
            this Maybe<T>                     a,
            Func<T, IEnumerable<TTempResult>> fn,
            Func<T, TTempResult, TResult>     composer) =>
            a.Match(d => fn(d).Select(tr => composer(d, tr)), () => Maybe<IEnumerable<TResult>>.Nothing);
    }
}