using System;
using Functional.Maybe;
using JetBrains.Annotations;

namespace Tauron
{
    [PublicAPI]
    public static class Maybe
    {
        public static Maybe<TValue> NotNull<TValue>(TValue? value)
            where TValue : class
            => value?.ToMaybe() ?? Maybe<TValue>.Nothing;


        public static Maybe<TValue> Cast<TValue>(object? value) => value is not TValue realValue ? Maybe<TValue>.Nothing : realValue.ToMaybe();

        public static Maybe<TValue> From<TValue>(Func<TValue?> factory)
        {
            var val = factory();
            return val is null ? Maybe<TValue>.Nothing : val.ToMaybe();
        }
    }

}