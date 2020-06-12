using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.Akka.ServiceResolver.Messages.Global
{
    [PublicAPI]
    public class QueryServiceResponse
    {
        public QueryServiceResponse(IActorRef? service)
        {
            Service = service;
        }

        public IActorRef? Service { get; }

        public bool Ok => Service != null;
    }
}