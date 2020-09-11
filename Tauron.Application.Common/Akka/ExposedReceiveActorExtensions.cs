using System;
using Akka.Event;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public static class ExposedReceiveActorExtensions
    {
        public static void SubscribeToEvent<TEvent>(this IExposedReceiveActor actor, Action<TEvent> handler) 
            => new EventHolder<TEvent>(actor, handler).Register();

        private sealed class EventHolder<TEvent>
        {
            private readonly Action<TEvent> _handler;
            private readonly IExposedReceiveActor _actor;

            public EventHolder(IExposedReceiveActor actor, Action<TEvent> handler)
            {
                _handler = handler;
                _actor = actor;
            }

            public void Register()
            {
                _actor.Exposed.Receive<TEvent>((e, c) => _handler(e));

                _actor.Exposed.OnPreStart += context => context.System.EventStream.Subscribe<TEvent>(context.Self);
                _actor.Exposed.OnPostStop += context => context.System.EventStream.Unsubscribe<TEvent>(context.Self);
            }
        }
    }
}