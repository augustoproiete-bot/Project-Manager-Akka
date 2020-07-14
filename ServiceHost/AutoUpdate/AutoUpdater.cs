using JetBrains.Annotations;
using Tauron.Akka;

namespace ServiceHost.AutoUpdate
{
    public sealed class AutoUpdater : DefaultActorRef<AutoUpdateActor>, IAutoUpdater
    {
        public AutoUpdater(ActorRefFactory<AutoUpdateActor> actorBuilder) 
            : base(actorBuilder) =>
            Init("Auto-Updater");
    }
}