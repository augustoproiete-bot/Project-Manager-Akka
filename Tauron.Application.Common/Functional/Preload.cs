using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Functional.Either;
using Functional.Maybe;
using JetBrains.Annotations;

namespace Tauron
{
    [PublicAPI]
    public static class Preload
    {
        public static TResult Throw<TResult>(Maybe<TResult> may, Func<Exception> error) 
            => may.OrElse(error);

        public static Maybe<TResult> MayThrow<TResult>(Maybe<TResult> may, Func<Exception> error)
        {
            if (may.IsNothing())
                throw error();
            return may;
        }

        public static Either<TResult, Exception> Try<TResult>(Func<TResult> action, Action? finaly = null)
        {
            try
            {
                return Either<TResult, Exception>.Result(action());
            }
            catch (Exception e)
            {
                return Either<TResult, Exception>.Error(e);
            }
            finally
            {
                finaly?.Invoke();
            }
        }

        public static Either<Unit, Exception> Try(Action action, Action? finaly = null)
        {
            try
            {
                action();
                return Either<Unit, Exception>.Result(Unit.Instance);
            }
            catch (Exception e)
            {
                return Either<Unit, Exception>.Error(e);
            }
            finally
            {
                finaly?.Invoke();
            }
        }

        public static void Finally(Action action, Action final)
        {
            try
            {
                action();
            }
            finally
            {
                final();
            }
        }

        public static TResult Finally<TResult>(Func<TResult> action, Action final)
        {
            try
            {
                return action();
            }
            finally
            {
                final();
            }
        }

        public static Maybe<TValue> May<TValue>(TValue value)
            => value.ToMaybe();

        public static Maybe<TValue> May<TValue>(TValue value, Func<TValue, bool> check)
        {
            if(!check(value)) return Maybe<TValue>.Nothing;
            return value.ToMaybe();
        }

        public static Maybe<TValue> May<TValue>(Func<TValue> value)
            => value().ToMaybe();

        public static Maybe<TValue> MayNotNull<TValue>(TValue? value)
            where TValue : class => value?.ToMaybe() ?? Maybe<TValue>.Nothing;

        public static Maybe<TValue> MayNotNull<TValue>(Func<TValue?> value)
            where TValue : class
            => value()?.ToMaybe() ?? Maybe<TValue>.Nothing;

        public static Task StartTask(Action action)
            => Task.Factory.StartNew(action);

        public static Task StartLongTask(Action action)
            => Task.Factory.StartNew(action, TaskCreationOptions.LongRunning);

        public static Unit Use(Action action)
        {
            action();
            return Unit.Instance;
        }

        public static TResult Use<TResult>(Func<TResult> action)
            => action();

        public static Maybe<Unit> MayUse(Action action)
        {
            action();
            return Unit.Instance.ToMaybe();
        }

        public static Maybe<TResult> MayUse<TResult>(Func<TResult> action)
            => action().ToMaybe();

        public static Maybe<T> Collapse<T>(Maybe<Maybe<T>> maybe)
            => maybe.Collapse();

        public static void Do<TResult>(Maybe<TResult> may) {}

        public static void Do<TResult>(Maybe<TResult> maybe, Action<TResult> doing)
            => maybe.Do(doing);

        public static void Do<TResult>(Maybe<Maybe<TResult>> may) { }

        public static void Do<TResult>(Maybe<Maybe<TResult>> maybe, Action<TResult> doing)
            => maybe.Collapse().Do(doing);

        public static void Match<TType>(Maybe<TType> may, Action<TType> some, Action non)
            => may.Match(some, non);
        public static Maybe<TResult> Match<TType, TResult>(Maybe<TType> may, Func<TType, TResult> some, Func<Maybe<TResult>> non)
            => may.Match(some, non);

        public static void Match<TType>(Maybe<Maybe<TType>> may, Action<TType> some, Action non)
            => may.Collapse().Match(some, non);
        public static Maybe<TResult> Match<TType, TResult>(Maybe<Maybe<TType>> may, Func<TType, TResult> some, Func<Maybe<TResult>> non)
            => may.Collapse().Match(some, non);

        public static TResult OrElse<TResult>(Maybe<TResult> may, TResult result)
            => may.OrElse(result);

        public static TResult OrElse<TResult>(Maybe<Maybe<TResult>> may, TResult result)
            => may.Collapse().OrElse(result);

        public static Maybe<TResult> Or<TResult>(Maybe<TResult> may, TResult res)
            => may.Or(res);

        public static Maybe<TResult> Or<TResult>(Maybe<TResult> may, Maybe<TResult> res)
            => may.Or(res);
        public static Maybe<TResult> Or<TResult>(Maybe<Maybe<TResult>> may, TResult res)
            => may.Collapse().Or(res);

        public static Maybe<TResult> Or<TResult>(Maybe<Maybe<TResult>> may, Maybe<TResult> res)
            => may.Collapse().Or(res);
        public static Maybe<TResult> Or<TResult>(Maybe<Maybe<TResult>> may, Maybe<Maybe<TResult>> res)
            => may.Collapse().Or(res.Collapse());

        public static Maybe<TResult> Or<TResult>(Maybe<TResult> may, Maybe<Maybe<TResult>> res)
            => may.Or(res.Collapse());

        public static Maybe<TResult> Any<TResult>(IEnumerable<Func<Maybe<TResult>>> maybes)
        {
            foreach (var maybeFunc in maybes)
            {
                var maybe = maybeFunc();
                if (maybe.IsSomething())
                    return maybe;
            }

            return Maybe<TResult>.Nothing;
        }

        public static Maybe<TResult> Any<TResult>(params Func<Maybe<TResult>>[] maybes) 
            => Any(maybes.AsEnumerable());

        public static Maybe<TResult> RunWith<TResult>(Func<Maybe<TResult>> mayBe, Action postRun)
        {
            var result = mayBe();
            if (result.IsNothing())
                return result;

            postRun();

            return result;
        }

        public static TResult RunWith<TResult>(Func<TResult> func, Action postRun)
        {
            var result = func();
            postRun();

            return result;
        }
    }
}