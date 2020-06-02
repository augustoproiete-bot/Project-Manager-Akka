using Akka.Actor;
using Akka.Event;
using Tauron.Application.Akka.ServiceResolver.Messages.Global;

namespace Tauron.Application.Akka.ServiceResolver.Actor
{
    public sealed class HostManagerActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly Props _service;
        private ToggleSuspendedMessage _suspendedMessage = new ToggleSuspendedMessage(false);

        public HostManagerActor(Props service)
        {
            _service = service;

            Receive<ToggleSuspendedMessage>(ToggleSuspendedMessage);
            Receive<QueryServiceRequest>(QueryServiceRequest);
        }

        private void QueryServiceRequest(QueryServiceRequest request)
        {
            var hostName = Context.Sender.Path.Address.Host ?? "Unkowen" + "-" +
                Context.Sender.Path.Address.System + "-Manager";

            _log.Info("Create or Return {Service}", hostName);
            var service = Context.GetOrCreate(hostName, Props.Create(() => new ServiceHostActor(_service)));
            service.Tell(_suspendedMessage);
            Context.Sender.Tell(new QueryServiceResponse(service));
        }

        private void ToggleSuspendedMessage(ToggleSuspendedMessage suspended)
        {
            _suspendedMessage = suspended;
            foreach (var actorRef in Context.GetChildren())
                actorRef.Tell(suspended);
        }
    }
}