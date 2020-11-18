using System;
using Akka.Actor;
using Functional.Maybe;
using JetBrains.Annotations;
using static Tauron.Prelude;

namespace Tauron.Akka
{
    [PublicAPI]
    public abstract class BaseActorRef<TActor>
        where TActor : ActorBase
    {
        private readonly ActorRefFactory<TActor> _builder;

        protected BaseActorRef(ActorRefFactory<TActor> actorBuilder)
            => _builder = actorBuilder;

        public Maybe<bool> IsInitialized { get; private set; } = May(false);

        protected virtual bool IsSync => false;

        public Maybe<IActorRef> Actor { get; private set; } = Maybe<IActorRef>.Nothing;

        public Maybe<ActorPath> Path => from act in Actor
                                        select act.Path;

        public event Action? Initialized;

        public void Tell(object message, IActorRef sender)
            => Do(Actor, a => a.Tell(message, sender));

        public bool Equals(IActorRef? other)
            => OrElse(from act in Actor
                      from otherAct in May(other)
                      where act.Equals(otherAct)
                      select true, false);

        public int CompareTo(IActorRef? other)
            => CompareTo(other as object);

        public int CompareTo(object? obj)
            => OrElse(from act in Actor
                      from other in May(obj)
                      select act.CompareTo(other), 0);

        public virtual void Init(string? name = null)
            => IniCore((b, s) => _builder.Create(b, s), name);

        public virtual void Init(IActorRefFactory factory, string? name = null)
        {
            IniCore((sync, parmName) => from prop in _builder.CreateProps(sync)
                                        select factory.ActorOf(prop, parmName), name);
        }

        protected virtual void IniCore(Func<bool, string?, Maybe<IActorRef>> create, string? name)
        {
            Actor =
                from _ in CheckIsInit()
                from actor in create(IsSync, name)
                select actor;

            IsInitialized = Or(from _ in Actor
                               select true, false);

            Do(from init in IsInitialized
               where init
               from initAction in MayNotNull(Initialized)
               select Use(initAction));
        }

        protected void ResetInternal()
        {
            IsInitialized = May(false);

            Do(from act in Actor
               select Use(() => act.Tell(PoisonPill.Instance)));

            Actor = Maybe<IActorRef>.Nothing;
        }

        protected Maybe<Unit> CheckIsInit()
        {
            if (IsInitialized.OrElse(false))
                throw new InvalidOperationException("ActorRef is Init");

            return Unit.MayInstance;
        }
    }
}