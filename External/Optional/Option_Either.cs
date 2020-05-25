﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Optional
{
    /// <summary>
    ///     Represents an optional value, along with a potential exceptional value.
    /// </summary>
    /// <typeparam name="T">The type of the value to be wrapped.</typeparam>
    /// <typeparam name="TException">A exceptional value describing the lack of an actual value.</typeparam>
    [Serializable, PublicAPI]
    [DebuggerTypeProxy(typeof(OptionDebugView<,>))]
    public readonly struct Option<T, TException> : IEquatable<Option<T, TException>>, IComparable<Option<T, TException>>
    {
        private readonly T _value;
        private readonly TException _exception;

        /// <summary>
        ///     Checks if a value is present.
        /// </summary>
        public bool HasValue { get; }

        [MaybeNull]
        internal T Value => _value;

        [MaybeNull]
        internal TException Exception => _exception;

        internal Option([AllowNull] T value, [AllowNull] TException exception, bool hasValue)
        {
            _value = value!;
            HasValue = hasValue;
            _exception = exception!;
        }

        /// <summary>
        ///     Determines whether two optionals are equal.
        /// </summary>
        /// <param name="other">The optional to compare with the current one.</param>
        /// <returns>A boolean indicating whether or not the optionals are equal.</returns>
        public bool Equals(Option<T, TException> other)
        {
            if (!HasValue && !other.HasValue)
                return EqualityComparer<TException>.Default.Equals(_exception, other._exception);
            if (HasValue && other.HasValue) return EqualityComparer<T>.Default.Equals(_value, other._value);

            return false;
        }

        /// <summary>
        ///     Determines whether two optionals are equal.
        /// </summary>
        /// <param name="obj">The optional to compare with the current one.</param>
        /// <returns>A boolean indicating whether or not the optionals are equal.</returns>
        public override bool Equals(object obj) => obj is Option<T, TException> option && Equals(option);

        /// <summary>
        ///     Determines whether two optionals are equal.
        /// </summary>
        /// <param name="left">The first optional to compare.</param>
        /// <param name="right">The second optional to compare.</param>
        /// <returns>A boolean indicating whether or not the optionals are equal.</returns>
        public static bool operator ==(Option<T, TException> left, Option<T, TException> right) => left.Equals(right);

        /// <summary>
        ///     Determines whether two optionals are unequal.
        /// </summary>
        /// <param name="left">The first optional to compare.</param>
        /// <param name="right">The second optional to compare.</param>
        /// <returns>A boolean indicating whether or not the optionals are unequal.</returns>
        public static bool operator !=(Option<T, TException> left, Option<T, TException> right) => !left.Equals(right);

        /// <summary>
        ///     Generates a hash code for the current optional.
        /// </summary>
        /// <returns>A hash code for the current optional.</returns>
        public override int GetHashCode()
        {
            if (HasValue)
            {
                if (_value == null) return 1;

                return _value.GetHashCode();
            }

            if (_exception == null) return 0;

            return _exception.GetHashCode();
        }


        /// <summary>
        ///     Compares the relative order of two optionals. An empty optional is
        ///     ordered by its exceptional value and always before a non-empty one.
        /// </summary>
        /// <param name="other">The optional to compare with the current one.</param>
        /// <returns>An integer indicating the relative order of the optionals being compared.</returns>
        public int CompareTo(Option<T, TException> other)
        {
            if (HasValue  && !other.HasValue) return 1;
            if (!HasValue && other.HasValue) return -1;

            return HasValue
                ? Comparer<T>.Default.Compare(_value, other._value)
                : Comparer<TException>.Default.Compare(_exception, other._exception);
        }

        /// <summary>
        ///     Determines if an optional is less than another optional.
        /// </summary>
        /// <param name="left">The first optional to compare.</param>
        /// <param name="right">The second optional to compare.</param>
        /// <returns>A boolean indicating whether or not the left optional is less than the right optional.</returns>
        public static bool operator <(Option<T, TException> left, Option<T, TException> right) => left.CompareTo(right) < 0;

        /// <summary>
        ///     Determines if an optional is less than or equal to another optional.
        /// </summary>
        /// <param name="left">The first optional to compare.</param>
        /// <param name="right">The second optional to compare.</param>
        /// <returns>A boolean indicating whether or not the left optional is less than or equal the right optional.</returns>
        public static bool operator <=(Option<T, TException> left, Option<T, TException> right) => left.CompareTo(right) <= 0;

        /// <summary>
        ///     Determines if an optional is greater than another optional.
        /// </summary>
        /// <param name="left">The first optional to compare.</param>
        /// <param name="right">The second optional to compare.</param>
        /// <returns>A boolean indicating whether or not the left optional is greater than the right optional.</returns>
        public static bool operator >(Option<T, TException> left, Option<T, TException> right) => left.CompareTo(right) > 0;

        /// <summary>
        ///     Determines if an optional is greater than or equal to another optional.
        /// </summary>
        /// <param name="left">The first optional to compare.</param>
        /// <param name="right">The second optional to compare.</param>
        /// <returns>A boolean indicating whether or not the left optional is greater than or equal the right optional.</returns>
        public static bool operator >=(Option<T, TException> left, Option<T, TException> right) => left.CompareTo(right) >= 0;

        /// <summary>
        ///     Returns a string that represents the current optional.
        /// </summary>
        /// <returns>A string that represents the current optional.</returns>
        public override string ToString()
        {
            if (HasValue) return _value == null ? "Some(null)" : $"Some({_value})";

            if (_exception == null) return "None(null)";

            return $"None({_exception})";
        }

        /// <summary>
        ///     Converts the current optional into an enumerable with one or zero elements.
        /// </summary>
        /// <returns>A corresponding enumerable.</returns>
        public IEnumerable<T> ToEnumerable()
        {
            if (HasValue) yield return _value;
        }

        /// <summary>
        ///     Returns an enumerator for the optional.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            if (HasValue) yield return _value;
        }

        /// <summary>
        ///     Determines if the current optional contains a specified value.
        /// </summary>
        /// <param name="value">The value to locate.</param>
        /// <returns>A boolean indicating whether or not the value was found.</returns>
        public bool Contains(T value)
        {
            if (HasValue)
            {
                if (_value == null) return value == null;

                return _value.Equals(value);
            }

            return false;
        }

        /// <summary>
        ///     Determines if the current optional contains a value
        ///     satisfying a specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A boolean indicating whether or not the predicate was satisfied.</returns>
        public bool Exists(Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return HasValue && predicate(_value);
        }

        /// <summary>
        ///     Returns the existing value if present, and otherwise an alternative value.
        /// </summary>
        /// <param name="alternative">The alternative value.</param>
        /// <returns>The existing or alternative value.</returns>
        public T ValueOr(T alternative) => HasValue ? _value : alternative;

        /// <summary>
        ///     Returns the existing value if present, and otherwise an alternative value.
        /// </summary>
        /// <param name="alternativeFactory">A factory function to create an alternative value.</param>
        /// <returns>The existing or alternative value.</returns>
        public T ValueOr(Func<T> alternativeFactory)
        {
            if (alternativeFactory == null) throw new ArgumentNullException(nameof(alternativeFactory));
            return HasValue ? _value : alternativeFactory();
        }

        /// <summary>
        ///     Returns the existing value if present, and otherwise an alternative value.
        /// </summary>
        /// <param name="alternativeFactory">A factory function to map the exceptional value to an alternative value.</param>
        /// <returns>The existing or alternative value.</returns>
        public T ValueOr(Func<TException, T> alternativeFactory)
        {
            if (alternativeFactory == null) throw new ArgumentNullException(nameof(alternativeFactory));
            return HasValue ? _value : alternativeFactory(_exception);
        }

        /// <summary>
        ///     Uses an alternative value, if no existing value is present.
        /// </summary>
        /// <param name="alternative">The alternative value.</param>
        /// <returns>A new optional, containing either the existing or alternative value.</returns>
        public Option<T, TException> Or(T alternative) => HasValue ? this : Option.Some<T, TException>(alternative);

        /// <summary>
        ///     Uses an alternative value, if no existing value is present.
        /// </summary>
        /// <param name="alternativeFactory">A factory function to create an alternative value.</param>
        /// <returns>A new optional, containing either the existing or alternative value.</returns>
        public Option<T, TException> Or(Func<T> alternativeFactory)
        {
            if (alternativeFactory == null) throw new ArgumentNullException(nameof(alternativeFactory));
            return HasValue ? this : Option.Some<T, TException>(alternativeFactory());
        }

        /// <summary>
        ///     Uses an alternative value, if no existing value is present.
        /// </summary>
        /// <param name="alternativeFactory">A factory function to map the exceptional value to an alternative value.</param>
        /// <returns>A new optional, containing either the existing or alternative value.</returns>
        public Option<T, TException> Or(Func<TException, T> alternativeFactory)
        {
            if (alternativeFactory == null) throw new ArgumentNullException(nameof(alternativeFactory));
            return HasValue ? this : Option.Some<T, TException>(alternativeFactory(_exception));
        }

        /// <summary>
        ///     Uses an alternative optional, if no existing value is present.
        /// </summary>
        /// <param name="alternativeOption">The alternative optional.</param>
        /// <returns>The alternative optional, if no value is present, otherwise itself.</returns>
        public Option<T, TException> Else(Option<T, TException> alternativeOption) => HasValue ? this : alternativeOption;

        /// <summary>
        ///     Uses an alternative optional, if no existing value is present.
        /// </summary>
        /// <param name="alternativeOptionFactory">A factory function to create an alternative optional.</param>
        /// <returns>The alternative optional, if no value is present, otherwise itself.</returns>
        public Option<T, TException> Else(Func<Option<T, TException>> alternativeOptionFactory)
        {
            if (alternativeOptionFactory == null) throw new ArgumentNullException(nameof(alternativeOptionFactory));
            return HasValue ? this : alternativeOptionFactory();
        }

        /// <summary>
        ///     Uses an alternative optional, if no existing value is present.
        /// </summary>
        /// <param name="alternativeOptionFactory">A factory function to map the exceptional value to an alternative optional.</param>
        /// <returns>The alternative optional, if no value is present, otherwise itself.</returns>
        public Option<T, TException> Else(Func<TException, Option<T, TException>> alternativeOptionFactory)
        {
            if (alternativeOptionFactory == null) throw new ArgumentNullException(nameof(alternativeOptionFactory));
            return HasValue ? this : alternativeOptionFactory(_exception);
        }

        /// <summary>
        ///     Forgets any attached exceptional value.
        /// </summary>
        /// <returns>An optional without an exceptional value.</returns>
        public Option<T> WithoutException()
        {
            return Match(
                Option.Some,
                _ => Option.None<T>()
            );
        }

        /// <summary>
        ///     Evaluates a specified function, based on whether a value is present or not.
        /// </summary>
        /// <param name="some">The function to evaluate if the value is present.</param>
        /// <param name="none">The function to evaluate if the value is missing.</param>
        /// <returns>The result of the evaluated function.</returns>
        public TResult Match<TResult>(Func<T, TResult> some, Func<TException, TResult> none)
        {
            if (some == null) throw new ArgumentNullException(nameof(some));
            if (none == null) throw new ArgumentNullException(nameof(none));
            return HasValue ? some(_value) : none(_exception);
        }

        /// <summary>
        ///     Evaluates a specified action, based on whether a value is present or not.
        /// </summary>
        /// <param name="some">The action to evaluate if the value is present.</param>
        /// <param name="none">The action to evaluate if the value is missing.</param>
        public void Match(Action<T> some, Action<TException> none)
        {
            if (some == null) throw new ArgumentNullException(nameof(some));
            if (none == null) throw new ArgumentNullException(nameof(none));

            if (HasValue)
                some(_value);
            else
                none(_exception);
        }

        /// <summary>
        ///     Evaluates a specified action if a value is present.
        /// </summary>
        /// <param name="some">The action to evaluate if the value is present.</param>
        public void MatchSome(Action<T> some)
        {
            if (some == null) throw new ArgumentNullException(nameof(some));

            if (HasValue) some(_value);
        }

        /// <summary>
        ///     Evaluates a specified action if no value is present.
        /// </summary>
        /// <param name="none">The action to evaluate if the value is missing.</param>
        public void MatchNone(Action<TException> none)
        {
            if (none == null) throw new ArgumentNullException(nameof(none));

            if (!HasValue) none(_exception);
        }

        /// <summary>
        ///     Transforms the inner value in an optional.
        ///     If the instance is empty, an empty optional is returned.
        /// </summary>
        /// <param name="mapping">The transformation function.</param>
        /// <returns>The transformed optional.</returns>
        public Option<TResult, TException> Map<TResult>(Func<T, TResult> mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));

            return Match(
                value => Option.Some<TResult, TException>(mapping(value)),
                Option.None<TResult, TException>
            );
        }

        /// <summary>
        ///     Transforms the exceptional value in an optional.
        ///     If the instance is not empty, no transformation is carried out.
        /// </summary>
        /// <param name="mapping">The transformation function.</param>
        /// <returns>The transformed optional.</returns>
        public Option<T, TExceptionResult> MapException<TExceptionResult>(Func<TException, TExceptionResult> mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));

            return Match(
                Option.Some<T, TExceptionResult>,
                exception => Option.None<T, TExceptionResult>(mapping(exception))
            );
        }

        /// <summary>
        ///     Transforms the inner value in an optional
        ///     into another optional. The result is flattened,
        ///     and if either is empty, an empty optional is returned.
        /// </summary>
        /// <param name="mapping">The transformation function.</param>
        /// <returns>The transformed optional.</returns>
        public Option<TResult, TException> FlatMap<TResult>(Func<T, Option<TResult, TException>> mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));

            return Match(
                mapping,
                Option.None<TResult, TException>
            );
        }

        /// <summary>
        ///     Transforms the inner value in an optional
        ///     into another optional. The result is flattened,
        ///     and if either is empty, an empty optional is returned,
        ///     with a specified exceptional value.
        /// </summary>
        /// <param name="mapping">The transformation function.</param>
        /// <param name="exception">The exceptional value to attach.</param>
        /// <returns>The transformed optional.</returns>
        public Option<TResult, TException> FlatMap<TResult>(Func<T, Option<TResult>> mapping, TException exception)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            return FlatMap(value => mapping(value).WithException(exception));
        }

        /// <summary>
        ///     Transforms the inner value in an optional
        ///     into another optional. The result is flattened,
        ///     and if either is empty, an empty optional is returned,
        ///     with a specified exceptional value.
        /// </summary>
        /// <param name="mapping">The transformation function.</param>
        /// <param name="exceptionFactory">A factory function to create an exceptional value to attach.</param>
        /// <returns>The transformed optional.</returns>
        public Option<TResult, TException> FlatMap<TResult>(Func<T, Option<TResult>> mapping, Func<TException> exceptionFactory)
        {
            if (mapping          == null) throw new ArgumentNullException(nameof(mapping));
            if (exceptionFactory == null) throw new ArgumentNullException(nameof(exceptionFactory));
            return FlatMap(value => mapping(value).WithException(exceptionFactory));
        }

        /// <summary>
        ///     Empties an optional, and attaches an exceptional value,
        ///     if a specified condition is not satisfied.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="exception">The exceptional value to attach.</param>
        /// <returns>The filtered optional.</returns>
        public Option<T, TException> Filter(bool condition, TException exception) =>
            HasValue && !condition ? Option.None<T, TException>(exception) : this;

        /// <summary>
        ///     Empties an optional, and attaches an exceptional value,
        ///     if a specified condition is not satisfied.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="exceptionFactory">A factory function to create an exceptional value to attach.</param>
        /// <returns>The filtered optional.</returns>
        public Option<T, TException> Filter(bool condition, Func<TException> exceptionFactory)
        {
            if (exceptionFactory == null) throw new ArgumentNullException(nameof(exceptionFactory));
            return HasValue && !condition ? Option.None<T, TException>(exceptionFactory()) : this;
        }

        /// <summary>
        ///     Empties an optional, and attaches an exceptional value,
        ///     if a specified predicate is not satisfied.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="exception">The exceptional value to attach.</param>
        /// <returns>The filtered optional.</returns>
        public Option<T, TException> Filter(Func<T, bool> predicate, TException exception)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return HasValue && !predicate(_value) ? Option.None<T, TException>(exception) : this;
        }

        /// <summary>
        ///     Empties an optional, and attaches an exceptional value,
        ///     if a specified predicate is not satisfied.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="exceptionFactory">A factory function to create an exceptional value to attach.</param>
        /// <returns>The filtered optional.</returns>
        public Option<T, TException> Filter(Func<T, bool> predicate, Func<TException> exceptionFactory)
        {
            if (predicate        == null) throw new ArgumentNullException(nameof(predicate));
            if (exceptionFactory == null) throw new ArgumentNullException(nameof(exceptionFactory));
            return HasValue && !predicate(_value) ? Option.None<T, TException>(exceptionFactory()) : this;
        }
    }

    [UsedImplicitly]
    internal sealed class OptionDebugView<T, TException>
    {
        private readonly Option<T, TException> _option;

        public OptionDebugView(Option<T, TException> option) => _option = option;

        public bool HasValue => _option.HasValue;
        public T Value => _option.Value!;
        public TException Exception => _option.Exception!;

        public override string ToString() => _option.ToString();
    }
}