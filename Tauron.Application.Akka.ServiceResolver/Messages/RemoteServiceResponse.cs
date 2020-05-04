using JetBrains.Annotations;
using Tauron.Application.Akka.ServiceResolver.Core;

namespace Tauron.Application.Akka.ServiceResolver.Messages
{
    [PublicAPI]
    public sealed class RemoteServiceResponse
    {
        public RemoteService Service { get; }

        public RemoteServiceResponse(RemoteService service) 
            => Service = service;
    }
}