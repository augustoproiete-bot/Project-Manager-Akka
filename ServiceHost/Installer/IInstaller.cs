using Tauron.Akka;

namespace ServiceHost.Installer
{
    public interface IInstaller : IDefaultActorRef<InstallManagerActor>
    {
        
    }
}