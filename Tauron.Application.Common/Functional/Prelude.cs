using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using Functional.Either;
using Functional.Maybe;
using JetBrains.Annotations;
using IOPath = System.IO.Path;
using IOFile = System.IO.File;
using IODic = System.IO.Directory;

namespace Tauron
{
    [PublicAPI]
    public static class Prelude
    {
        public static FuncLog To(ILoggingAdapter adapter)
            => new(adapter);

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

        public static Maybe<IActorRef> MayActor(IActorRef? actor)
            => actor.IsNobody() ? Maybe<IActorRef>.Nothing : actor.ToMaybe()!;

        public static Maybe<TValue> May<TValue>(TValue value)
            => value.ToMaybe();

        public static Maybe<TValue> May<TValue>(TValue value, Func<TValue, bool> check)
        {
            if (!check(value)) return Maybe<TValue>.Nothing;
            return value.ToMaybe();
        }

        public static Maybe<TValue> May<TValue>(Func<TValue> value)
            => value().ToMaybe();

        public static Maybe<TValue> MayNotNull<TValue>(TValue? value)
            where TValue : class => value?.ToMaybe() ?? Maybe<TValue>.Nothing;

        public static Maybe<string> MayNotEmpty(string? value)
            => string.IsNullOrWhiteSpace(value) ? Maybe<string>.Nothing : value.ToMaybe();

        public static Maybe<TValue> MayNotNull<TValue>(Func<TValue?> value)
            where TValue : class
            => value()?.ToMaybe() ?? Maybe<TValue>.Nothing;

        public static Task StartTask(Action action)
            => Task.Factory.StartNew(action);

        public static Task StartLongTask(Action action)
            => Task.Factory.StartNew(action, TaskCreationOptions.LongRunning);

        public static Unit Tell(IActorRef actor, object msg)
        {
            if (!actor.IsNobody())
                actor.Tell(msg);
            return Unit.Instance;
        }

        public static Unit Tell(IActorRef actor, object msg, IActorRef sender)
        {
            if (!actor.IsNobody())
                actor.Tell(msg, sender);
            return Unit.Instance;
        }

        public static Unit Tell(Maybe<IActorRef> actor, object msg, IActorRef sender) => actor.IsNothing() ? Unit.Instance : Tell(actor.Value, msg, sender);

        public static Unit Tell<TMsg>(IActorRef actor, Maybe<TMsg> msg)
        {
            if (!actor.IsNobody() && msg.IsSomething())
                actor.Tell(msg.Value);
            return Unit.Instance;
        }

        public static Unit Tell(Maybe<IActorRef> actor, object msg) => actor.IsNothing() ? Unit.Instance : Tell(actor.Value, msg);

        public static Unit Tell<TMsg>(Maybe<IActorRef> actor, Maybe<TMsg> msg) => actor.IsNothing() ? Unit.Instance : Tell(actor.Value, msg);

        public static Maybe<Unit> MayTell(IActorRef actor, object msg)
        {
            Tell(actor, msg);
            return Unit.MayInstance;
        }

        public static Maybe<Unit> MayTell(IActorRef actor, object msg, IActorRef sender)
        {
            Tell(actor, msg, sender);
            return Unit.MayInstance;
        }

        public static Maybe<Unit> MayTell(Maybe<IActorRef> actor, object msg, IActorRef sender)
        {
            Tell(actor, msg, sender);
            return Unit.MayInstance;
        }

        public static Maybe<Unit> MayTell<TMsg>(IActorRef actor, Maybe<TMsg> msg)
        {
            Tell(actor, msg);
            return Unit.MayInstance;
        }

        public static Maybe<Unit> MayTell(Maybe<IActorRef> actor, object msg)
        {
            Tell(actor, msg);
            return Unit.MayInstance;
        }

        public static Maybe<Unit> MayTell<TMsg>(Maybe<IActorRef> actor, Maybe<TMsg> msg)
        {
            Tell(actor, msg);
            return Unit.MayInstance;
        }

        public static Maybe<Unit> MayTell(object msg, Maybe<IActorRef> actor)
        {
            Tell(actor, msg);
            return Unit.MayInstance;
        }

        public static Maybe<Unit> MayTell<TMsg>(Maybe<TMsg> msg, Maybe<IActorRef> actor)
        {
            Tell(actor, msg);
            return Unit.MayInstance;
        }

        public static Unit Tell(object msg, Maybe<IActorRef> actor) => Tell(actor, msg);

        public static Unit Tell<TMsg>(Maybe<TMsg> msg, Maybe<IActorRef> actor) => actor.IsNothing() ? Unit.Instance : Tell(actor.Value, msg);

        public static Task<TResult> Ask<TResult>(IActorRef actor, object msg, TimeSpan? timeout = null) 
            => actor.Ask<TResult>(msg, timeout);

        public static Task<TResult> Ask<TResult>(Maybe<IActorRef> actor, object msg, TimeSpan? timeout = null) 
            => actor.IsNothing() 
                ? Task.FromException<TResult>(new InvalidOperationException("Try to Ask an Empty Maybe Actor")) 
                : Ask<TResult>(actor.Value, msg, timeout);

        public static Unit Forward(IActorRef actor, object msg)
        {
            if (!actor.IsNobody())
                actor.Forward(msg);
            return Unit.Instance;
        }

        public static Maybe<Unit> Forward(Maybe<IActorRef> actor, object msg)
        {
            if(actor.IsNothing())
                return Maybe<Unit>.Nothing;

            Forward(actor.Value, msg);

            return Unit.MayInstance;
        }

        public static Unit Action(Action action)
        {
            action();
            return Unit.Instance;
        }

        public static Maybe<Unit> MayAction(Action action)
        {
            action();
            return Unit.MayInstance;
        }

        public static TResult Func<TResult>(Func<TResult> action)
            => action();

        public static Maybe<TResult> MayFunc<TResult>(Func<TResult> action)
            => May(action());

        public static Maybe<Unit> MayUse(Action action)
        {
            action();
            return Unit.Instance.ToMaybe();
        }

        public static Maybe<TResult> MayUse<TResult>(Func<TResult> action)
            => action().ToMaybe();

        public static Maybe<T> Collapse<T>(Maybe<Maybe<T>> maybe)
            => maybe.Collapse();

        public static TData DoModify<TData>(TData data, Func<Maybe<TData>, Maybe<TData>> modify)
        {
            var result = modify(data.ToMaybe());

            return result.IsSomething() ? result.Value : data;
        }

        public static void Do<TResult>(Maybe<TResult> may)
        {
        }
        
        public static Task DoAsync(Maybe<Task> may) 
            => may.IsNothing() ? Task.CompletedTask : may.Value;

        public static void Do<TResult>(Maybe<TResult> maybe, Action<TResult> doing)
            => maybe.Do(doing);

        public static void Do<TResult>(Maybe<Maybe<TResult>> may)
        {
        }

        public static void Do<TResult>(Maybe<Maybe<TResult>> maybe, Action<TResult> doing)
            => maybe.Collapse().Do(doing);

        public static void Match<TType>(Maybe<TType> may, Action<TType> some, Action non)
            => may.Match(some, non);

        public static Maybe<TResult> Match<TType, TResult>(Maybe<TType> may, Func<TType, TResult> some, Func<Maybe<TResult>> non)
            => may.Match(some, non);

        public static Maybe<TResult> Match<TType, TResult>(Maybe<TType> may, Func<TType, TResult> some, Func<TResult> non)
            => may.Match(some, () => non().ToMaybe());

        public static Task<TResult> MatchAsync<TType, TResult>(Maybe<TType> may, Func<TType, Task<TResult>> some, Func<Task<TResult>> non)
            => may.MatchAsync(some, non);


        public static Maybe<TResult> Match<TType, TError, TResult>(Either<TType, TError> may, Func<TType, Maybe<TResult>> some, Func<TError, Maybe<TResult>> non)
            => may.Match(some, non);

        public static Maybe<TType> Match<TType, TError>(Either<Maybe<TType>, TError> may, Func<TError, Maybe<TType>> non)
            => may.Match(v => v, non);
        
        public static Maybe<TType> Match<TType, TError>(Either<TType, TError> may, Func<TError, TType> non)
            => May(may.Match(v => v, non));
        
        public static Maybe<TType> Match<TType, TError>(Either<Maybe<TType>, TError> may, Func<TError, TType> non)
            => may.Match(v => v, e => May(non(e)));

        public static Maybe<TType> MatchMay<TType, TError>(Maybe<Either<TType, TError>> may, Func<TError, TType> non)
        {
            if(may.IsNothing())
                return Maybe<TType>.Nothing;
            return may.Value.Match(May, error =>  May(non(error)));
        }

        public static Maybe<TType> MatchMay<TType, TError>(Maybe<Either<Maybe<TType>, TError>> may, Func<TError, TType> non)
        {
            if (may.IsNothing())
                return Maybe<TType>.Nothing;
            return may.Value.Match(r => r, error => May(non(error)));
        }

        public static TResult OrElse<TResult>(Maybe<TResult> may, TResult result)
            => may.OrElse(result);
        
        public static TResult OrElse<TResult>(Maybe<Maybe<TResult>> may, TResult result)
            => may.Collapse().OrElse(result);


        public static Maybe<TResult> Either<TResult>(Maybe<TResult> may, TResult res)
            => may.Or(res);

        public static Maybe<TResult> Either<TResult>(Maybe<TResult> may, Maybe<TResult> res)
            => may.Or(res);

        public static Maybe<TResult> Either<TResult>(Func<Maybe<TResult>> may1, Func<Maybe<TResult>> may2)
            => may1().Or(may2);

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


        public static Maybe<Unit> WaitTask(Task target)
        {
            target.Wait();
            return Unit.MayInstance;
        }

        public static class IO
        {
            [PublicAPI]
            public static class Path
            {
                public static Maybe<string> GetDirectoryName(Maybe<string> mayDic)
                    => from dic in mayDic
                       select MayNotNull(IOPath.GetDirectoryName(dic));

                public static Maybe<string> GetFullPath(Maybe<string> mayPath)
                    => from path in mayPath
                       select IOPath.GetFullPath(path);
            }

            [PublicAPI]
            public static class Directory
            {
                public static Maybe<bool> Exists(Maybe<string> mayPath)
                    => from path in mayPath
                       select IODic.Exists(path);

                public static Maybe<DirectoryInfo> CreateDirectory(Maybe<string> mayPath)
                    => from path in mayPath
                       select IODic.CreateDirectory(path);
            }

            [PublicAPI]
            public static class File
            {
                public static Maybe<bool> Exists(Maybe<string> mayPath)
                    => from path in mayPath
                       select IOFile.Exists(path);

                public static Unit WriteAllText(string fileName, string content)
                {
                    IOFile.WriteAllText(fileName, content);
                    return Unit.Instance;
                }

                public static Maybe<TResult> Open<TResult>(Maybe<string> path, FileMode mode, Func<Maybe<FileStream>, Maybe<TResult>> process)
                {
                    if(path.IsNothing())
                        return Maybe<TResult>.Nothing;
                    using var stream = IOFile.Open(path.Value, mode);
                    return process(May(stream));
                }
            }
        }
    }
}