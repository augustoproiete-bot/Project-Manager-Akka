using System.Diagnostics.CodeAnalysis;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.Akka.ServiceResolver.Messages.Global
{
    [PublicAPI]
    public class QueryServiceResponse
    {
        public IActorRef? Service { get; }

        public bool Ok => Service == null;

        public QueryServiceResponse(IActorRef? service) => Service = service;
    }
}