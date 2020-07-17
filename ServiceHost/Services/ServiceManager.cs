using JetBrains.Annotations;
using ServiceHost.Services.Impl;
using Tauron.Akka;

namespace ServiceHost.Services
{
    public sealed class ServiceManager : DefaultActorRef<ServiceManagerActor>, IServiceManager
    {
        public ServiceManager(ActorRefFactory<ServiceManagerActor> actorBuilder) 
            : base(actorBuilder)
        {
            Init("Service-Manager");
        }
    }
}