using Akka.Actor;
using Akka.Cluster.Utility;
using JetBrains.Annotations;

namespace Tauron.Application.Master.Commands
{
    [PublicAPI]
    public sealed class ServiceRegistry
    {
        private readonly IActorRef _target;

        private ServiceRegistry(IActorRef target) => _target = target;

        public static void Start(IActorRefFactory factory)
        {
            
        }

        private sealed class ServiceRegistryActor : ReceiveActor
        {
            
        }
    }
}