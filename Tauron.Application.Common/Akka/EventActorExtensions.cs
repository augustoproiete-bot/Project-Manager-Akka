using System;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public static class EventActorExtensions
    {
        public static IEventActor Create(this IActorRefFactory system, bool killOnFirstResponse = false)
            => EventActor.Create(system, killOnFirstResponse);

        public static IEventActor Create<TPayload>(this IActorRefFactory system, Action<TPayload> handler, bool killOnFirstResponse = false) 
            => EventActor.Create(system, handler, killOnFirstResponse);
    }
}