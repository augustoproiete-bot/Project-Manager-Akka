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
        public static Maybe<Task<TResult>> Ask<TResult>(this IInitableActorRef model, object message, TimeSpan? timeout = null)
            => from act in model.Actor
               select act.Ask<TResult>(message, timeout);

        public static Maybe<Unit> Tell(this IInitableActorRef model, object msg)
            => Tell(model, msg, ActorRefs.NoSender);

        public static Maybe<Unit> Tell(this IInitableActorRef model, object msg, IActorRef sender)
            => from act in model.Actor
               select Use(() => act.Tell(msg, sender));
    }
}