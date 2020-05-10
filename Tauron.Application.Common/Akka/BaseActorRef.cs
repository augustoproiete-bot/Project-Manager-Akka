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
        private IActorRef _ref = ActorRefs.Nobody;

        public event Action? Initialized;

        public bool IsInitialized { get; private set; }

        protected BaseActorRef(ActorRefFactory<TActor> actorBuilder)
            => _builder = actorBuilder;

        protected virtual bool IsSync => false;

        public IActorRef Actor => _ref;

        public void Tell(object message, IActorRef sender) => _ref.Tell(message, sender);

        public bool Equals(IActorRef? other) => _ref.Equals(other);

        public int CompareTo(IActorRef? other) => _ref.CompareTo(other);

        public int CompareTo(object? obj) => _ref.CompareTo(obj);

        public ActorPath Path => _ref.Path;

        public virtual void Init(string? name = null)
        {
            CheckIsInit();
            _ref = _builder.Create(IsSync, name);
            IsInitialized = true;
            Initialized?.Invoke();
        }

        public virtual void Init(IActorRefFactory factory, string? name = null)
        {
            CheckIsInit();
            _ref = factory.ActorOf(_builder.CreateProps(IsSync), name);
            IsInitialized = true;
            Initialized?.Invoke();
        }

        protected void ResetInternal()
        {
            _ref = ActorRefs.Nobody;
            IsInitialized = false;
        }

        protected void CheckIsInit()
        {
            if (IsInitialized)
                throw new InvalidOperationException("ActorRef is Init");
        }
    }
}