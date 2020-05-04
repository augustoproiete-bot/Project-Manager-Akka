using System.Collections.Generic;
using Akka.Actor;
using Tauron.Application.Akka.ServiceResolver.Core;
using Tauron.Application.Akka.ServiceResolver.Messages;

namespace Tauron.Application.Akka.ServiceResolver.Actor
{
    public sealed class RemoteServiceActor : ReceiveActor
    {
        private readonly Dictionary<string, RemoteService> _services = new Dictionary<string, RemoteService>();

        public RemoteServiceActor()
        {
            Receive<RemoteServiceRequest>(RemoteServiceRequest);
            Receive<Terminated>(_ => {});
            Receive<RemoteService.ServiceTerminated>(Terminated);
        }

        private void Terminated(RemoteService.ServiceTerminated obj) 
            => _services.Remove(obj.Name);

        private void RemoteServiceRequest(RemoteServiceRequest request)
        {
            if (_services.TryGetValue(request.Name, out var service)) 
                Context.Sender.Tell(new RemoteServiceResponse(service));
            else
            {
                service = new RemoteService(Context.System, request.Name, Context);
                _services[request.Name] = service;
                Context.Sender.Tell(new RemoteServiceResponse(service));
            }
        }
    }
}