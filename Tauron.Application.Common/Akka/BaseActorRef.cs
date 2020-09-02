using System;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public abstract class BaseActorRef<TActor>
        where TActor : ActorBase
    {
        private readonly ActorRefFactory<TActor> _builder;

        protected BaseActorRef(ActorRefFactory<TActor> actorBuilder)
        {
            _builder = actorBuilder;
        }

        public bool IsInitialized { get; private set; }

        protected virtual bool IsSync => false;

        public IActorRef Actor { get; private set; } = ActorRefs.Nobody;

        public ActorPath Path => Actor.Path;

        public event Action? Initialized;

        public void Tell(object message, IActorRef sender)
        {
            Actor.Tell(message, sender);
        }

        public bool Equals(IActorRef? other)
        {
            return Actor.Equals(other);
        }

        public int CompareTo(IActorRef? other)
        {
            return Actor.CompareTo(other);
        }

        public int CompareTo(object? obj)
        {
            return Actor.CompareTo(obj);
        }

        public virtual void Init(string? name = null)
        {
            CheckIsInit();
            Actor = _builder.Create(IsSync, name);
            IsInitialized = true;
            Initialized?.Invoke();
        }

        public virtual void Init(IActorRefFactory factory, string? name = null)
        {
            CheckIsInit();
            Actor = factory.ActorOf(_builder.CreateProps(IsSync), name);
            IsInitialized = true;
            Initialized?.Invoke();
        }

        protected void ResetInternal()
        {
            Actor.Tell(PoisonPill.Instance);
            Actor = ActorRefs.Nobody;
            IsInitialized = false;
        }

        protected void CheckIsInit()
        {
            if (IsInitialized)
                throw new InvalidOperationException("ActorRef is Init");
        }
    }
}