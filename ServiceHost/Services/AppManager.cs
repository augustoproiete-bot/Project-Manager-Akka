using JetBrains.Annotations;
using ServiceHost.Services.Impl;
using Tauron.Akka;

namespace ServiceHost.Services
{
    public sealed class AppManager : DefaultActorRef<AppManagerActor>, IAppManager
    {
        public AppManager(ActorRefFactory<AppManagerActor> actorBuilder) 
            : base(actorBuilder)
        {
            Init("Service-Manager");
        }
    }
}