using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using Tauron.Application.Akka.ServiceResolver.Core;
using Tauron.Application.Akka.ServiceResolver.Messages;

namespace Tauron.Application.Akka.ServiceResolver.Actor
{
    public sealed class RemoteServiceActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly Dictionary<string, RemoteService> _services = new Dictionary<string, RemoteService>();

        public RemoteServiceActor()
        {
            Receive<RemoteServiceRequest>(RemoteServiceRequest);
            Receive<Terminated>(_ => {});
            Receive<RemoteService.ServiceTerminated>(Terminated);
        }

        private void Terminated(RemoteService.ServiceTerminated obj)
        {
            _log.Info("Service Terminated {Name}", obj.Name);
            _services.Remove(obj.Name);
        }

        private void RemoteServiceRequest(RemoteServiceRequest request)
        {
            _log.Info("Service Requested {Name}", request.Name);
            if (_services.TryGetValue(request.Name, out var service))
            {
                _log.Info("Service Found {Name}", request.Name);
                Context.Sender.Tell(new RemoteServiceResponse(service));
            }
            else
            {
                _log.Info("Create Service {Name}", request.Name);
                service = new RemoteService(Context.System, request.Name, Context, _log);
                _services[request.Name] = service;
                Context.Sender.Tell(new RemoteServiceResponse(service));
            }
        }
    }
}