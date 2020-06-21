using System.Collections.Generic;
using Akka.Actor;
using Akka.Cluster;

namespace Tauron.Application.Master.Commands
{
    public sealed class QueryRegistratedServices
    {
        public IActorRef Sender { get; }

        public QueryRegistratedServices(IActorRef sender) => Sender = sender;
    }

    public sealed class QueryRegistratedServicesResponse
    {
        public Dictionary<UniqueAddress, string> Services { get; }

        public QueryRegistratedServicesResponse(Dictionary<UniqueAddress, string> services) => Services = services;
    }
}