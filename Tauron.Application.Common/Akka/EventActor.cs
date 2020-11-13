using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using Functional.Maybe;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public sealed class EventActor : UntypedActor
    {
        private readonly bool _killOnFirstRespond;
        private readonly ILoggingAdapter _log = Context.GetLogger();

        private readonly Dictionary<Type, Delegate> _registrations = new();

        private EventActor(bool killOnFirstRespond) => _killOnFirstRespond = killOnFirstRespond;

        public static Maybe<IEventActor> From(IActorRef actorRef) 
            => new HookEventActor(actorRef).ToMaybe<IEventActor>();

        public static Maybe<IEventActor> Create(IActorRefFactory system, Maybe<string> name, bool killOnFirstResponse = false)
        {
            return
                from realName in name.Or(string.Empty.ToMaybe())
                let actor = system.ActorOf(Props.Create(() => new EventActor(killOnFirstResponse)), realName)
                select (IEventActor)new HookEventActor(actor);
        }

        public static Maybe<IEventActor> Create<TPayload>(IActorRefFactory system, Action<TPayload> handler, bool killOnFirstResponse = false)
        {
            var temp = Create(system, Maybe<string>.Nothing, killOnFirstResponse);
            temp.Do(a => a.Register(HookEvent.Create(handler)));
            return temp;
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case HookEvent hookEvent:

                    var targetDelegate =
                        from del in _registrations.Lookup(hookEvent.Target)
                        select Delegate.Combine(del, hookEvent.Invoker);

                    _registrations.AddOrSet(hookEvent.Target, targetDelegate.OrElse(hookEvent.Invoker));
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

                        if (_killOnFirstRespond)
                            Context.Stop(Context.Self);
                    }
                    else
                    {
                        Unhandled(message);
                    }

                    break;
            }
        }

        private sealed class HookEventActor : IEventActor
        {
            public HookEventActor(IActorRef actorRef) 
                => OriginalRef = actorRef;

            public IActorRef OriginalRef { get; }

            public void Register(HookEvent hookEvent)
                => OriginalRef.Tell(hookEvent);

            public void Send(IActorRef actor, object send) 
                => actor.Tell(send, OriginalRef);
        }
    }
}