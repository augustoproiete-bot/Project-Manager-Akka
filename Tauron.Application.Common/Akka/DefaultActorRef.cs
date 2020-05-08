namespace Tauron.Akka
{
    public sealed class DefaultActorRef<TActor> : BaseActorRef<TActor>, IDefaultActorRef<TActor>
    {
        public DefaultActorRef(ActorRefFactory<TActor> actorBuilder) 
            : base(actorBuilder)
        {
        }
    }
}