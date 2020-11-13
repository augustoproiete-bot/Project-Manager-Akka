using System;
using Akka.Actor;
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

    [PublicAPI]
    public static class MaybeExtensions
    {
        public static Maybe<TOut> Match<TIn, TOut>(this Maybe<TIn> mayInput, Func<TIn, TOut> mach, Func<Maybe<TOut>> onElse)
            => mayInput.IsNothing()
                ? onElse()
                : from input in mayInput
                  select mach(input);

        public static void MayTell<TMsg>(this IActorRef actor, Maybe<TMsg> mayData)
            => mayData.Do(m => actor.Tell(m));

        public static void MayTell<TMsg>(this IActorRef actor, Maybe<TMsg> mayData, IActorRef sender)
            => mayData.Do(m => actor.Tell(m, sender));
    }
}