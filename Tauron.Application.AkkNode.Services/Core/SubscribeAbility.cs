using System;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron.Application.Master.Commands.Core
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

        private IUntypedActorContext ActorContext => _actor.ExposedContext;

        public event Action<Terminated>? Terminated;

        private GroupDictionary<Type, IActorRef> _sunscriptions = new GroupDictionary<Type, IActorRef>();

        public SubscribeAbility(ExposedReceiveActor actor) 
            => _actor = actor;

        public void MakeReceive()
        { 
            _actor.Flow<Terminated>()
                .From.Action(t => Terminated?.Invoke(t))
                .AndReceive();

            _actor.Flow<KeyHint>()
                .From.Action(kh => _sunscriptions.Remove(kh.Key, kh.Target))
                .AndReceive();

            _actor.Flow<Subscribe>()
                .From.Action(s =>
                {
                    _actor.ExposedContext
                        .Sender.When(ar => !ar.Equals(ActorRefs.Nobody),
                            sender =>
                            {
                                s.When(subscribe => subscribe.Watch,
                                    () => ActorContext.WatchWith(sender, new KeyHint(sender, s.Event)));
                                _sunscriptions.Add(s.Event, sender);
                            });
                })
                .AndReceive();

            _actor.Flow<UnSubscribe>()
                .From.Action(s =>
                {
                    _actor.ExposedContext
                        .Sender.When(ar => !ar.Equals(ActorRefs.Nobody),
                            sender =>
                            {
                                ActorContext.Unwatch(sender);
                                _sunscriptions.Remove(s.Event, sender);
                            });
                })
                .AndReceive();
        }

        public TType Send<TType>(TType payload)
        {
            if (!_sunscriptions.TryGetValue(typeof(TType), out var coll)) return payload;
            
            foreach (var actorRef in coll) actorRef.Tell(payload);
            return payload;
        }
    }

    [PublicAPI]
    public sealed class UnSubscribe
    {
        public Type Event { get; }

        public UnSubscribe(Type @event) => Event = @event;
    }

    [PublicAPI]
    public sealed class Subscribe
    {
        public bool Watch { get; }

        public Type Event { get; }

        public Subscribe(bool watch, Type @event)
        {
            Watch = watch;
            Event = @event;
        }

        public Subscribe(Type @event)
        {
            Event = @event;
            Watch = true;
        }
    }
}