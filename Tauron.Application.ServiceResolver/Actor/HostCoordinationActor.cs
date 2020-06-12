using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using Tauron.Akka;
using Tauron.Application.Akka.ServiceResolver.Core;
using Tauron.Application.Akka.ServiceResolver.Messages;
using Tauron.Application.Akka.ServiceResolver.Messages.Global;

namespace Tauron.Application.Akka.ServiceResolver.Actor
{
    public sealed class HostCoordinationActor : ResolveActorBase
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly Dictionary<string, Props> _services = new Dictionary<string, Props>();
        private ToggleSuspendedMessage _suspendedMessage = new ToggleSuspendedMessage(false);

        private ICanTell? _tracker;

        public HostCoordinationActor()
        {
            Receive<RegistrationRejectedMessage>(_ => Context.System.Terminate());
            Receive<ToggleSuspendedMessage>(ToggleSuspendedMessage);
            Receive<QueryServiceRequest>(QueryServiceRequest);
            Receive<RegisterServices>(RegisterServicesHandler);
            Receive<HostLocalServiceMessage>(HostLocalServiceMessage);
        }

        private void HostLocalServiceMessage(HostLocalServiceMessage obj)
        {
            _log.Info("Create Local Host {Name}", obj.Name);
            HostLocalServiceResponse response;

            if (_services.ContainsKey(obj.Name))
            {
                _log.Error("Local Host Name is Registrated {Name}", obj.Name);
                response = new HostLocalServiceResponse(null);
            }
            else
            {
                response = new HostLocalServiceResponse(Context.ActorOf(Props.Create(() => new ServiceHostActor(obj.Props)), obj.Name));
            }

            Context.Sender.Tell(response);
        }


        private void RegisterServicesHandler(RegisterServices obj)
        {
            _log.Info("Register Services");

            _tracker = obj.Config.Tracker.Combine(new Tracker(obj.Config.InterfaceTracker));
            foreach (var (name, props) in obj.Config.Services)
            {
                _log.Debug("Service {Service}", name);
                _services[name] = props;
            }
        }

        private void QueryServiceRequest(QueryServiceRequest obj)
        {
            _log.Info("Incommign Query Service Request: {Service}", obj.Name);
            if (_services.TryGetValue(obj.Name, out var value))
            {
                _log.Info("Service Found");
                var actor = Context.GetOrAdd(obj.Name, Props.Create(() => new HostManagerActor(value)));
                actor.Tell(_suspendedMessage);
                actor.Forward(obj);
            }
            else
            {
                _log.Warning("Service not Found {Service}", obj.Name);
                obj.Sender.Tell(new QueryServiceResponse(null));
            }
        }

        private void ToggleSuspendedMessage(ToggleSuspendedMessage obj)
        {
            _log.Info("Suspended Message {State}", obj.IsSuspended);

            _suspendedMessage = obj;
            foreach (var child in Context.GetChildren())
                child.Forward(obj);

            _tracker?.Tell(obj, Context.Sender);
        }

        private sealed class Tracker : ICanTell
        {
            private readonly ISuspensionTracker? _tarcker;

            public Tracker(ISuspensionTracker? tarcker) => _tarcker = tarcker;

            public void Tell(object message, IActorRef sender)
            {
                if (message is ToggleSuspendedMessage msg)
                    _tarcker?.Suspended(msg);
            }
        }

        public sealed class RegisterServices
        {
            public RegisterServices(EndpointConfig services) => Config = services;

            public EndpointConfig Config { get; }
        }
    }
}