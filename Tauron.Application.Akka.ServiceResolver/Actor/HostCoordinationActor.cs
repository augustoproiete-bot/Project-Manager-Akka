using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using Tauron.Application.Akka.ServiceResolver.Messages.Global;

namespace Tauron.Application.Akka.ServiceResolver.Actor
{
    public sealed class HostCoordinationActor : ResolveActorBase
    {
        public sealed class RegisterServices
        {
            public IReadOnlyList<(string Name, Props Props)> Services { get; }

            public RegisterServices(IReadOnlyList<(string Name, Props Props)> services) 
                => Services = services;
        }

        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly Dictionary<string, Props> _services = new Dictionary<string, Props>();
        
        public HostCoordinationActor()
        {
            Receive<RegistrationRejectedMessage>(_ => Context.System.Terminate());
            Receive<ToggleSuspendedMessage>(ToggleSuspendedMessage);
            Receive<QueryServiceRequest>(QueryServiceRequest);
            Receive<RegisterServices>(RegisterServicesHandler);dfg
        }


        private void RegisterServicesHandler(RegisterServices obj)
        {
            
            foreach (var (name, props) in obj.Services) 
                _services[name] = props;
        }

        private void QueryServiceRequest(QueryServiceRequest obj)
        {
            if (_services.TryGetValue(obj.Name, out var value)) 
                Context.GetOrCreate(obj.Name, Props.Create(() => new HostManagerActor(value))).Forward(obj);
            else
                Context.Sender.Tell(new QueryServiceResponse(null));
        }

        private void ToggleSuspendedMessage(ToggleSuspendedMessage obj)
        {
            foreach (var child in Context.GetChildren()) 
                child.Forward(obj);
        }
    }
}
