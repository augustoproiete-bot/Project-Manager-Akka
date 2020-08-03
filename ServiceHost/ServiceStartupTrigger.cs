using Akka.Actor;
using Akka.Cluster;
using ServiceHost.Services;
using Tauron.Akka;
using Tauron.Application.AkkaNode.Boottrap;
using Tauron.Application.Master.Commands.Host;

namespace ServiceHost
{
    public sealed class ServiceStartupTrigger : IStartUpAction
    {
        private readonly IAppManager _manager;
        private readonly ActorSystem _system;

        public ServiceStartupTrigger(IAppManager manager, ActorSystem system)
        {
            _manager = manager;
            _system = system;
        }

        public void Run()
        {
            _manager.Tell(new StartApps(AppType.StartUp));
            Cluster.Get(_system).RegisterOnMemberUp(() => _manager.Tell(new StartApps(AppType.Cluster)));
        }
    }
}