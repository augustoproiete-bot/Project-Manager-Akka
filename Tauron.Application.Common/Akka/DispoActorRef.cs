using System;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    public sealed class DispoActorRef<TActor> : BaseActorRef<TActor>, IDisposable where TActor : ActorBase
    {
        public DispoActorRef([NotNull] ActorRefFactory<TActor> actorBuilder) : base(actorBuilder)
        {
        }

        public void Dispose() 
            => Tell(PoisonPill.Instance, ActorRefs.NoSender);
    }
}