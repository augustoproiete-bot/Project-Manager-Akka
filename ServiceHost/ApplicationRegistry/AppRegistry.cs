using Tauron.Akka;

namespace ServiceHost.ApplicationRegistry
{
    public sealed class AppRegistry : DefaultActorRef<AppRegistryActor>, IAppRegistry
    {
        public AppRegistry(ActorRefFactory<AppRegistryActor> actorBuilder) : base(actorBuilder) => Init("Apps-Registry");
    }
}