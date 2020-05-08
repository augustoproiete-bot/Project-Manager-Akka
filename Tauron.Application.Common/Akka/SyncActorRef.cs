using JetBrains.Annotations;

namespace Tauron.Akka
{
    public class SyncActorRef<TActor> : BaseActorRef<TActor>, ISyncActorRef<TActor>
    {
        public SyncActorRef([NotNull] ActorRefFactory<TActor> actorBuilder) : base(actorBuilder)
        {
        }

        protected override bool IsSync => true;
    }
}