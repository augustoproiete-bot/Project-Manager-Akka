

using System;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Akka.ServiceResolver.Actor;
using Tauron.Application.Akka.ServiceResolver.Messages.Global;

namespace Tauron.Application.Akka.ServiceResolver.Core
{
    [PublicAPI]
    public sealed class ResolverExt : IExtension
    {
        private readonly ExtendedActorSystem _system;

        public ResolverExt(ExtendedActorSystem system)
        {
            _system = system;
            Settings = new ResolverSettings(system.Settings.Config);
            GlobalResolver = system.ActorOf(Props.Create(() => new GlobalResolver()), "GlobalResolver");
            HostActor = system.ActorOf(Props.Create(() => new HostCoordinationActor()), "HostCoordination");
            RemoteServiceActor = system.ActorOf<RemoteServiceActor>("RemoteServices");
        }

        public IActorRef GlobalResolver { get; }

        public IActorRef HostActor { get; }

        public IActorRef RemoteServiceActor { get; }

        public ResolverSettings Settings { get; }

        internal void Initialize()
        {
            GlobalResolver.Tell(new GlobalResolver.Initialize(Settings));
        }

        public void RegisterEndpoint(EndpointConfig config)
        {
            GlobalResolver.Tell(new GlobalResolver.RegisterEndpoint(config.Services.Select(t => t.Key).ToArray(), config.ServiceRequirement, HostActor));
            HostActor.Tell(new HostCoordinationActor.RegisterServices(config));
        }

        public Task<QueryServiceResponse> GetServiceEntry(string name)
        {
            return GlobalResolver.Ask<QueryServiceResponse>(new QueryServiceRequest(name), TimeSpan.FromSeconds(10));
        }
    }
}