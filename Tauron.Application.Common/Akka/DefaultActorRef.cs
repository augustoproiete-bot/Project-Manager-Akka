using Akka.Actor;

namespace Tauron.Akka
{
    public class DefaultActorRef<TActor> : BaseActorRef<TActor>, IDefaultActorRef<TActor> where TActor : ActorBase
    {
        public DefaultActorRef(ActorRefFactory<TActor> actorBuilder)
            : base(actorBuilder)
        {
        }
    }
}