using Akka.Actor;

namespace Tauron.Application.Akka.ServiceResolver.Messages
{
    public sealed class HostLocalServiceResponse
    {
        public HostLocalServiceResponse(IActorRef? service)
        {
            Service = service;
        }

        public IActorRef? Service { get; }
    }
}