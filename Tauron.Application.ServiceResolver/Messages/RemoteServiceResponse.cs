using JetBrains.Annotations;
using Tauron.Application.Akka.ServiceResolver.Core;

namespace Tauron.Application.Akka.ServiceResolver.Messages
{
    [PublicAPI]
    public sealed class RemoteServiceResponse
    {
        public RemoteServiceResponse(RemoteService service)
        {
            Service = service;
        }

        public RemoteService Service { get; }
    }
}