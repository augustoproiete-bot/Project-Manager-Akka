using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using Functional.Maybe;
using JetBrains.Annotations;
using static Tauron.Preload;

namespace Tauron.Akka
{
    [PublicAPI]
    public sealed class EventActor : UntypedActor
    {
        private readonly bool _killOnFirstRespond;
        private readonly ILoggingAdapter _log = Context.GetLogger();

        private ImmutableDictionary<Type, Delegate> _registrations = ImmutableDictionary<Type, Delegate>.Empty;

        private EventActor(bool killOnFirstRespond) => _killOnFirstRespond = killOnFirstRespond;

        public static Maybe<IEventActor> From(IActorRef actorRef) 
            => new HookEventActor(actorRef).ToMaybe<IEventActor>();

        public static Maybe<IEventActor> Create(IActorRefFactory system, Maybe<string> name, bool killOnFirstResponse = false)
        {
            return from realName in name.Or(string.Empty.ToMaybe())
                   let actor = system.ActorOf(Props.Create(() => new EventActor(killOnFirstResponse)), realName)
                   select (IEventActor)new HookEventActor(actor);
        }

        public static Maybe<IEventActor> Create<TPayload>(IActorRefFactory system, Action<TPayload> handler, bool killOnFirstResponse = false)
        {
            return from actor in Create(system, Maybe<string>.Nothing, killOnFirstResponse)
                   select actor.Register(HookEvent.Create(handler));
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case HookEvent hookEvent:
                    var targetDelegate =
                        from del in _registrations.Lookup(hookEvent.Target)
                        let combDel = Delegate.Combine(del, hookEvent.Invoker)
                        select _registrations.Add(hookEvent.Target, combDel);

                    _registrations = targetDelegate.OrElse(_registrations);
                    break;
                default:
                    var msg = from msgType in May(message.GetType())
                              from registration in _registrations.Lookup(msgType)
                              select Try(() => registration.DynamicInvoke(message));

                    Match(msg,
                        mayE => mayE.OnError(e => _log.Error(e, "Error On Event Hook Execution")),
                        () => Unhandled(msg));
                    break;
            }
        }

        private sealed class HookEventActor : IEventActor
        {
            public HookEventActor(IActorRef actorRef) 
                => OriginalRef = actorRef;

            public IActorRef OriginalRef { get; }

            public IEventActor Register(HookEvent hookEvent)
            {
                OriginalRef.Tell(hookEvent);
                return this;
            }

            public IEventActor Send(IActorRef actor, object send)
            {
                actor.Tell(send, OriginalRef);
                return this;
            }
        }
    }
}