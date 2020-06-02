using System;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public static class EventActorExtensions
    {
        public static IEventActor CreateEventActor(this IActorRefFactory system, bool killOnFirstResponse = false)
        {
            return EventActor.Create(system, killOnFirstResponse);
        }

        public static IEventActor CreateEventActor<TPayload>(this IActorRefFactory system, Action<TPayload> handler, bool killOnFirstResponse = false)
        {
            return EventActor.Create(system, handler, killOnFirstResponse);
        }
    }
}