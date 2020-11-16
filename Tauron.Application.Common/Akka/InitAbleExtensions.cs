using System;
using System.Threading.Tasks;
using Akka.Actor;
using Functional.Maybe;
using JetBrains.Annotations;
using static Tauron.Preload;

namespace Tauron.Akka
{
    [PublicAPI]
    public static class InitableExtensions
    {
        public static async Task<TResult> Ask<TResult>(this IInitableActorRef model, object message, TimeSpan? timeout = null)
        {
            return await MatchAsync(model.Actor,
                r => r.Ask<TResult>(message, timeout),
                () => Task.FromException<TResult>(new InvalidOperationException("Actor not Initialized")));
        }

        public static Maybe<Unit> Tell(this IInitableActorRef model, object msg)
            => Tell(model, msg, ActorRefs.NoSender);

        public static Maybe<Unit> Tell(this IInitableActorRef model, object msg, IActorRef sender)
            => from act in model.Actor
               select Use(() => act.Tell(msg, sender));
    }
}