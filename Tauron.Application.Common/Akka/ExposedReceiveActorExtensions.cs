using System;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public static class ExposedReceiveActorExtensions
    {
        public static FSMBase.State<TS, TD> Replying<TS, TD>(this FSMBase.State<TS, TD> state, object msg, IActorRef actor)
        {
            actor.Tell(msg);
            return state;
        }

        public static FSMBase.State<TS, TD> ReplyingSelf<TS, TD>(this FSMBase.State<TS, TD> state, object msg) => state.Replying(msg, ExposedReceiveActor.ExposedContext.Self);

        public static FSMBase.State<TS, TD> ReplyingParent<TS, TD>(this FSMBase.State<TS, TD> state, object msg) => state.Replying(msg, ExposedReceiveActor.ExposedContext.Parent);

        public static void SubscribeToEvent<TEvent>(this IExposedReceiveActor actor, Action<TEvent> handler)
            => new EventHolder<TEvent>(actor, handler).Register();

        private sealed class EventHolder<TEvent>
        {
            private readonly IExposedReceiveActor _actor;
            private readonly Action<TEvent>       _handler;

            public EventHolder(IExposedReceiveActor actor, Action<TEvent> handler)
            {
                _handler = handler;
                _actor   = actor;
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