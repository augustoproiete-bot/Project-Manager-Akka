using System;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron.Application.AkkNode.Services.Core
{
    [PublicAPI]
    public sealed class SubscribeAbility
    {
        private sealed class KeyHint
        {
            public IActorRef Target { get; }

            public Type Key { get; }

            public KeyHint(IActorRef target, Type key)
            {
                Target = target;
                Key = key;
            }
        }

        private readonly ExposedReceiveActor _actor;

        private IUntypedActorContext ActorContext => ExposedReceiveActor.ExposedContext;

        public event Action<Terminated>? Terminated;

        public event Action<InternalEventSubscription>? NewSubscription; 

        private GroupDictionary<Type, IActorRef> _sunscriptions = new GroupDictionary<Type, IActorRef>();

        public SubscribeAbility(ExposedReceiveActor actor) 
            => _actor = actor;

        public void MakeReceive()
        {
            _actor.Flow<Terminated>(b => b.Action(t => Terminated?.Invoke(t)));

            _actor.Flow<KeyHint>(b => b.Action(kh => _sunscriptions.Remove(kh.Key, kh.Target)));

            _actor.Flow<EventSubscribe>(b =>
                b.Action(s =>
                {
                    ExposedReceiveActor.ExposedContext
                        .Sender.When(ar => !ar.Equals(ActorRefs.Nobody),
                            sender =>
                            {
                                s.When(subscribe => subscribe.Watch,
                                    () => ActorContext.WatchWith(sender, new KeyHint(sender, s.Event)));
                                _sunscriptions.Add(s.Event, sender);
                                NewSubscription?.Invoke(new InternalEventSubscription(sender, s.Event));
                            });
                }));

            _actor.Flow<EventUnSubscribe>(b =>
                b.Action(s =>
                {
                    ExposedReceiveActor.ExposedContext
                        .Sender.When(ar => !ar.Equals(ActorRefs.Nobody),
                            sender =>
                            {
                                ActorContext.Unwatch(sender);
                                _sunscriptions.Remove(s.Event, sender);
                            });
                }));
        }

        public TType Send<TType>(TType payload)
        {
            if (!_sunscriptions.TryGetValue(typeof(TType), out var coll)) return payload;
            
            foreach (var actorRef in coll) actorRef.Tell(payload);
            return payload;
        }

        public TType Send<TType>(TType payload, Type evtType)
        {
            if (!_sunscriptions.TryGetValue(evtType, out var coll)) return payload;

            foreach (var actorRef in coll) actorRef.Tell(payload);
            return payload;
        }

        public sealed class InternalEventSubscription
        {
            public IActorRef Intrest { get; }

            public Type Type { get; }

            public InternalEventSubscription(IActorRef intrest, Type type)
            {
                this.Intrest = intrest;
                Type = type;
            }
        }
    }

    [PublicAPI]
    public sealed class EventUnSubscribe
    {
        public Type Event { get; }

        public EventUnSubscribe(Type @event) => Event = @event;
    }

    [PublicAPI]
    public sealed class EventSubscribe
    {
        public bool Watch { get; }

        public Type Event { get; }

        public EventSubscribe(bool watch, Type @event)
        {
            Watch = watch;
            Event = @event;
        }

        public EventSubscribe(Type @event)
        {
            Event = @event;
            Watch = true;
        }
    }
}