using System;
using JetBrains.Annotations;

namespace Tauron
{
    [PublicAPI]
    public static class F
    {
        public static Func<TInput, TOutput> To<TInput, TMiddle, TOutput>(this Func<TInput, TMiddle> input, Func<TMiddle, TOutput> output)
            => i => output(input(i));

        public static Func<TOutput> To<TMiddle, TOutput>(this Func<TMiddle> input, Func<TMiddle, TOutput> output)
            => () => output(input());

        public static Func<TOutput> CloseFunc<TInput, TOutput>(this Func<TInput, TOutput> output, Func<TInput> toClose)
            => () => output(toClose());
    }
}