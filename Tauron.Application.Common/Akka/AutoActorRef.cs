using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    public sealed class AutoActorRef<TActor> : BaseActorRef<TActor>, IAutoActorRef<TActor> where TActor : ActorBase
    {
        public AutoActorRef([NotNull] ActorRefFactory<TActor> actorBuilder) 
            : base(actorBuilder) 
            => Init();
    }
}