using Tauron.Akka;

namespace ServiceHost.AutoUpdate
{
    public interface IAutoUpdater : IDefaultActorRef<AutoUpdateActor>
    {
        
    }
}