using Akka.Actor;
using Tauron.Akka;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.Core.Model
{
    public sealed class DeploymentServices
    {
        private readonly IActorRef _manager;

        public DeploymentServices(ActorSystem system)
        {
            _manager = system.ActorOf<ServiceManager>("Deployment_Service_Manager");
        }

        private sealed class ServiceManager : ExposedReceiveActor
        {
            
        }
    }
}