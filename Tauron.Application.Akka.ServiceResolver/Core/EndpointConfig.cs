using System.Collections.Generic;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Akka.ServiceResolver.Data;

namespace Tauron.Application.Akka.ServiceResolver.Core
{
    [PublicAPI]
    public class EndpointConfig
    {
        internal EndpointConfig()
        {
        }

        public static EndpointConfig New => new EndpointConfig();

        internal IActorRef? Tracker { get; set; }

        internal ISuspensionTracker? InterfaceTracker { get; set; }

        internal Dictionary<string, Props> Services { get; } = new Dictionary<string, Props>();

        internal ServiceRequirement ServiceRequirement { get; set; } = ServiceRequirement.Empty;

        public EndpointConfig WithSuspensionTracker(IActorRef tracker)
        {
            Tracker = tracker;
            return this;
        }

        public EndpointConfig WithSuspensionTracker(ISuspensionTracker tracker)
        {
            InterfaceTracker = tracker;
            return this;
        }

        public EndpointConfig WithServices(params (string Name, Props Service)[] services)
        {
            foreach (var (name, service) in services)
                Services[name] = service;

            return this;
        }

        public EndpointConfig WithServiceRequirement(ServiceRequirement requirement)
        {
            ServiceRequirement = requirement;
            return this;
        }
    }
}