using Akka.Actor;
using Tauron.Application.Akka.ServiceResolver.Messages.Global;

namespace Tauron.Application.Akka.ServiceResolver.Actor
{
    public sealed class EndPointManager : ReceiveActor
    {
        public sealed class EndpointLostMessage
        {

        }

        public sealed class ServiceAddedMessage
        {

        }

        public sealed class ServiceLostMessage
        {

        }

        private readonly IActorRef _targetEndpoint;
        private readonly RegisterEndpointMessage _message;

        public EndPointManager(IActorRef targetEndpoint,)
        {
            _targetEndpoint = targetEndpoint;
            Context.Watch(_targetEndpoint);

            Receive<Terminated>(Terminated);
        }

        private void Terminated(Terminated obj)
        {
            
        }
    }
}