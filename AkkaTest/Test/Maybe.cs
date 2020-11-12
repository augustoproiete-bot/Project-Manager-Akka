using System;
using JetBrains.Annotations;

namespace AkkaTest.Test
{
    public interface IMaybe<TValue>
    {
        bool HasValue { get; }

        TValue? Value { get; }

        void OnValue(Action<TValue> valueAction);

        void OnNothing(Action action);

        Maybe<TResult> Map<TResult>(Func<TValue, TResult?> map);

        Maybe<TResult> Bind<TResult>(Func<TValue, Maybe<TResult>> map);

        Maybe<TValue> AsNothing();

        Maybe<TValue> AsMaybe();
    }

    [PublicAPI]
    public readonly struct Maybe<TValue> : IMaybe<TValue>
    {
        public Maybe(TValue? value, bool hasValue)
        {
            Value = value;
            HasValue = hasValue;
        }

        public bool HasValue { get; }

        public TValue? Value { get; }

        public void OnValue(Action<TValue> valueAction)
        {
            if (Value != null)
                valueAction(Value);
        }

        public void OnNothing(Action action)
        {
            if (Value == null)
                action();
        }

        public Maybe<TResult> Map<TResult>(Func<TValue, TResult?> map) 
            => HasValue && Value is not null ? new(map(Value), true) : new();

        public Maybe<TResult> Bind<TResult>(Func<TValue, Maybe<TResult>> map)
            => HasValue && Value is not null ? map(Value) : new();

        public Maybe<TValue> AsNothing()
            => new();

        public Maybe<TValue> AsMaybe() => this;

        public Maybe<TValue> WithValue(TValue value)
            => new(value, true);
    }

    [PublicAPI]
    public static class Maybe
    {
        public static Maybe<TValue> Just<TValue>(TValue? value)
        {
            if (value is null)
                return new();
            return new(value, true);
        }

        public static Maybe<TValue> Nothing<TValue>()
            => new();

        public static Maybe<TValue> From<TValue>(Func<TValue?> factory)
        {
            var val = factory();
            if (val is null)
                return new();
            return new(val, true);
        }
    }

    public static class MaybeExtensions
    {
        public static Maybe<TResult> Select<TValue, TResult>(this IMaybe<TValue> source, Func<TValue, TResult> selector)
            => source.Map(selector);

        public static Maybe<TValue> Where<TValue>(this IMaybe<TValue> value, Predicate<TValue> predicate)
            => value.Bind(v => predicate(v) ? value.AsMaybe() : value.AsNothing());

        //public static Maybe<(TValue1, TValue2)> Combine<TValue1, TValue2>(this IMaybe<TValue1> first, IMaybe<TValue2> second)
        //{
        //    if (first.HasValue  && first.Value is not null &&
        //        second.HasValue && second.Value is not null)
        //        return new((first.Value, second.Value), true);

        //    return new();
        //}
    }
}