using System;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Akka.ServiceResolver.Actor;
using Tauron.Application.Akka.ServiceResolver.Core;
using Tauron.Application.Akka.ServiceResolver.Messages.Global;

namespace Tauron.Application.Akka.ServiceResolver
{
    [PublicAPI]
    public sealed class ServiceResolver : IExtension
    {
        private readonly ExtendedActorSystem _system;

        public ServiceResolver(ExtendedActorSystem system)
        {
            _system = system;
            Settings = new ResolverSettings(system.Settings.Config);
            GlobalResolver = system.ActorOf(Props.Create(() => new GlobalResolver()), "GlobalResolver");
            HostActor = system.ActorOf(Props.Create(() => new HostCoordinationActor()), "HostCoordination");
            RemoteServiceActor = system.ActorOf<RemoteServiceActor>("RemoteServices");

            GlobalResolver.Tell(new GlobalResolver.Initialize(Settings));
        }

        public IActorRef GlobalResolver { get; }

        public IActorRef HostActor { get; }

        public IActorRef RemoteServiceActor { get; }

        public ResolverSettings Settings { get; }

        public void RegisterEndpoint(EndpointConfig config)
        {
            GlobalResolver.Tell(new GlobalResolver.RegisterEndpoint(config.Services.Select(t => t.Key).ToArray(), config.ServiceRequirement, HostActor));
            HostActor.Tell(new HostCoordinationActor.RegisterServices(config));
        }

        public Task<QueryServiceResponse> GetServiceEntry(string name) 
            => GlobalResolver.Ask<QueryServiceResponse>(new QueryServiceRequest(name), TimeSpan.FromSeconds(10));

        public static ServiceResolver Get(ActorSystem system)
            => system.WithExtension<ServiceResolver, ServiceResolverProvider>();
    }
}