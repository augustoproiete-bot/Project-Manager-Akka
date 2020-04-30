using System.Collections.Generic;
using Akka.Actor;
using Tauron.Application.Akka.ServiceResolver.Data;

namespace Tauron.Application.Akka.ServiceResolver.Actor
{
    public sealed class EndPointManager : ReceiveActor
    {
        public sealed class EndpointLostMessage
        {

        }

        public sealed class ServiceChangeMessages
        {
            public IReadOnlyList<string> AllServices { get; }

            public ServiceChangeMessages(IReadOnlyList<string> allServices) => AllServices = allServices;
        }

        public sealed class ToggleSuspendedMessage
        {

        }

        private readonly IActorRef _targetEndpoint;
        private readonly ServiceRequirement _requirement;
        private bool _isSuspended;

        public EndPointManager(IActorRef targetEndpoint, ServiceRequirement requirement)
        {
            _targetEndpoint = targetEndpoint;
            _requirement = requirement;
            Context.Watch(_targetEndpoint);

            Receive<Terminated>(Terminated);
            Receive<ServiceChangeMessages>(ServiceChangeMessagesHandler);
        }

        private void ServiceChangeMessagesHandler(ServiceChangeMessages obj)
        {
            var ok = _requirement.IsDefiend(obj.AllServices);
            if(_isSuspended == ok) return;

            _isSuspended = ok;

            var sus = new ToggleSuspendedMessage();
            _targetEndpoint.Tell(sus);
            Context.Parent.Tell(sus);
        }

        private void Terminated(Terminated obj)
        {
            Context.Parent.Tell(new EndpointLostMessage());
            Context.Self.Tell(PoisonPill.Instance);
        }
    }
}