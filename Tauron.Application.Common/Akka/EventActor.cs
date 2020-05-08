using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public sealed class EventActor : UntypedActor
    {
        private sealed class HookEventActor : IEventActor
        {
            private readonly IActorRef _actorRef;

            public HookEventActor(IActorRef actorRef) 
                => _actorRef = actorRef;

            public void Register(HookEvent hookEvent) 
                => _actorRef.Tell(hookEvent);

            public void Send(IActorRef actor, object send) 
                => actor.Tell(send, _actorRef);
        }

        public static IEventActor Create(IActorRefFactory system, bool killOnFirstResponse = false) 
            => new HookEventActor(system.ActorOf(Props.Create(() => new EventActor(killOnFirstResponse))));

        private readonly Dictionary<Type, Delegate> _registrations = new Dictionary<Type, Delegate>();
        private readonly bool _killOnFirstRespond;
        private readonly ILoggingAdapter _log = Context.GetLogger();

        public EventActor(bool killOnFirstRespond) 
            => _killOnFirstRespond = killOnFirstRespond;

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case HookEvent hookEvent:
                    if (_registrations.TryGetValue(hookEvent.Target, out var del))
                        del = Delegate.Combine(del, hookEvent.Invoker);
                    else
                        del = hookEvent.Invoker;

                    _registrations[hookEvent.Target] = del;
                    break;
                default:
                    var msgType = message.GetType();
                    if (_registrations.TryGetValue(msgType, out var callDel))
                    {
                        try
                        {
                            callDel.DynamicInvoke(message);
                        }
                        catch (Exception e)
                        {
                            _log.Error(e, "Error On Event Hook Execution");
                        }

                        if(_killOnFirstRespond)
                            Context.Self.Tell(PoisonPill.Instance);
                    }
                    else
                        Unhandled(message);
                    break;
            }
        }
    }
}