using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using Tauron.Application.Akka.ServiceResolver.Data;
using Tauron.Application.Akka.ServiceResolver.Messages.Global;

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

        private readonly ILoggingAdapter _log;
        private readonly IActorRef _targetEndpoint;
        private readonly ServiceRequirement _requirement;
        private bool _isSuspended;

        public EndPointManager(IActorRef targetEndpoint, ServiceRequirement requirement)
        {
            _log = Context.GetLogger();

            _targetEndpoint = targetEndpoint;
            _requirement = requirement;
            Context.Watch(_targetEndpoint);

            Receive<Terminated>(Terminated);
            Receive<ServiceChangeMessages>(ServiceChangeMessagesHandler);
            Receive<QueryServiceRequest>(QueryServiceRequest);
        }

        private void QueryServiceRequest(QueryServiceRequest obj) 
            => Context.Sender.Tell(new QueryServiceResponse(_targetEndpoint));

        private void ServiceChangeMessagesHandler(ServiceChangeMessages obj)
        {
            var ok = !_requirement.IsDefiend(obj.AllServices);
            if(_isSuspended == ok) return;

            _log.Info("Switch Service Suspension From {Current} To {New} On {Endpoint}", ok, !ok, _targetEndpoint.Path);
            _isSuspended = ok;

            var sus = new ToggleSuspendedMessage(_isSuspended);
            _targetEndpoint.Tell(sus);
            Context.Parent.Tell(sus);
        }

        private void Terminated(Terminated obj)
        {
            _log.Warning("Endpoint Conecction Lost {Endpoint}", _targetEndpoint.Path);
            Context.Parent.Tell(new EndpointLostMessage());
            Context.Self.Tell(PoisonPill.Instance);
        }
    }
}