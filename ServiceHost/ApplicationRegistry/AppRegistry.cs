using Akka.Actor;
using JetBrains.Annotations;
using ServiceHost.ApplicationRegistry.Core;
using Tauron.Akka;

namespace ServiceHost.ApplicationRegistry
{
    public interface IAppRegistry : IDefaultActorRef<ApplicationManager>
    {
        
    }

    internal sealed class AppRegistry : DefaultActorRef<ApplicationManager>, IAppRegistry
    {
        public AppRegistry(ActorRefFactory<ApplicationManager> actorBuilder, ActorSystem system) 
            : base(actorBuilder)
        {
            Init(system, "App-Registry");
        }
    }
}