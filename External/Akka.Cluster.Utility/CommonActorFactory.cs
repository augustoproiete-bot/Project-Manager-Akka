using Akka.Actor;
using JetBrains.Annotations;

namespace Akka.Cluster.Utility
{
    [PublicAPI]
    public class CommonActorFactory<TActor> : IActorFactory
         where TActor : ActorBase
    {
        public void Initialize(object[] args)
        {
        }

        public IActorRef CreateActor(IActorRefFactory actorRefFactory, object id, object[] args) 
            => actorRefFactory.ActorOf(Props.Create<TActor>(args), id.ToString());
    }
}
