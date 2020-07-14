using JetBrains.Annotations;
using Tauron.Akka;

namespace ServiceHost.Installer
{
    public sealed class Installer : DefaultActorRef<InstallManagerActor>, IInstaller
    {
        public Installer(ActorRefFactory<InstallManagerActor> actorBuilder) : base(actorBuilder) => Init("Installer");
    }
}