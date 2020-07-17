using ServiceHost.Services.Impl;
using Tauron.Akka;

namespace ServiceHost.Services
{
    public interface IAppManager : IDefaultActorRef<AppManagerActor>
    {
        
    }
}