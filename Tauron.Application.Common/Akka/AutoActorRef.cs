using JetBrains.Annotations;

namespace Tauron.Akka
{
    public sealed class AutoActorRef<TActor> : BaseActorRef<TActor>, IAutoActorRef<TActor>
    {
        public AutoActorRef([NotNull] ActorRefFactory<TActor> actorBuilder) 
            : base(actorBuilder) 
            => Init();
    }
}