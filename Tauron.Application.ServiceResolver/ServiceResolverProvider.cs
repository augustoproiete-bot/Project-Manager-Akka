using Akka.Actor;

namespace Tauron.Application.Akka.ServiceResolver
{
    public class ServiceResolverProvider : ExtensionIdProvider<ServiceResolver>
    {
        public override ServiceResolver CreateExtension(ExtendedActorSystem system) => new ServiceResolver(system);
    }
}