using Akka.Actor;
using Tauron.Application.Akka.ServiceResolver.Messages.Global;

namespace Tauron.Application.Akka.ServiceResolver.Actor
{
    public sealed class HostManagerActor : ReceiveActor
    {
        private readonly Props _service;

        public HostManagerActor(Props service)
        {
            _service = service;

            Receive<ToggleSuspendedMessage>(ToggleSuspendedMessage);
            Receive<QueryServiceRequest>(QueryServiceRequest);
        }

        private void QueryServiceRequest(QueryServiceRequest request)
        {
            var hostName = Context.Sender.Path.Address.Host ?? string.Empty + 
                Context.Sender.Path.Address.System;

            var service = Context.Child(hostName);
            if (service.Equals(ActorRefs.Nobody))
                service = Context.ActorOf(Props.Create(() => new ServiceHostActor(_service)), hostName);

            Context.Sender.Tell(new QueryServiceResponse(service));
        }

        private void ToggleSuspendedMessage(ToggleSuspendedMessage suspended)
        {
            foreach (var actorRef in Context.GetChildren()) 
                actorRef.Tell(suspended);
        }

        protected override SupervisorStrategy SupervisorStrategy() => new OneForOneStrategy(e => Directive.Restart);
    }
}