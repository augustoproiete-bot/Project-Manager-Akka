using Tauron.Akka;

namespace ServiceHost.ApplicationRegistry
{
    public interface IAppRegistry : IDefaultActorRef<AppRegistryActor>
    {
        
    }
}