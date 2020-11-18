using System;
using Akka.Actor;
using Functional.Maybe;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public static class EventActorExtensions
    {
        public static Maybe<IEventActor> CreateEventActor(this IActorRefFactory system, bool killOnFirstResponse = false)
            => EventActor.Create(system, Maybe<string>.Nothing, killOnFirstResponse).ToMaybe();

        public static Maybe<IEventActor> CreateEventActor<TPayload>(this IActorRefFactory system, Action<TPayload> handler, bool killOnFirstResponse = false)
            => EventActor.Create(system, handler, killOnFirstResponse).ToMaybe();

        public static Maybe<IEventActor> CreateEventActor(this IActorRefFactory system, Maybe<string> name, bool killOnFirstResponse = false)
            => EventActor.Create(system, name, killOnFirstResponse).ToMaybe();

        public static Maybe<IEventActor> GetOrCreateEventActor(this IUntypedActorContext system, Maybe<string> name, bool killOnFirstResponse = false)
            => from realName in name
               from child in system.TryGetChild(name)
               select child.IsNobody()
                          ? EventActor.Create(system, name, killOnFirstResponse)
                          : EventActor.From(child);
    }
}