using Akka.Actor;

namespace Tauron.Application.Akka.ServiceResolver.Messages
{
    public sealed class HostLocalServiceResponse
    {
        public IActorRef? Service { get; }

        public HostLocalServiceResponse(IActorRef? service)
            => Service = service;
    }
}