using System;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Akka.ServiceResolver.Actor;
using Tauron.Application.Akka.ServiceResolver.Data;
using Tauron.Application.Akka.ServiceResolver.Messages.Global;

namespace Tauron.Application.Akka.ServiceResolver.Core
{
    [PublicAPI]
    public sealed class ResolverExt : IExtension
    {
        private readonly ExtendedActorSystem _system;

        public IActorRef GlobalResolver { get; } 

        public IActorRef HostActor { get; }

        public IActorRef RemoteServiceActor { get; }

        public ResolverSettings Settings { get; }

        public ResolverExt(ExtendedActorSystem system)
        {
            _system = system;
            Settings = new ResolverSettings(system.Settings.Config);
            GlobalResolver = system.ActorOf(Props.Create(() => new GlobalResolver()), "GlobalResolver");
            HostActor = system.ActorOf(Props.Create(() => new HostCoordinationActor()), "ServiceHost");
            RemoteServiceActor = system.ActorOf<RemoteServiceActor>("RemoteServices");
        }

        internal void Initialize() 
            => GlobalResolver.Tell(new GlobalResolver.Initialize(Settings));

        public void RegisterEndpoint(ServiceRequirement requirement, params (string, Props)[] hostServices)
        {
            GlobalResolver.Tell(new GlobalResolver.RegisterEndpoint(hostServices.Select(t => t.Item1).ToArray(), requirement, HostActor));
            HostActor.Tell(new HostCoordinationActor.RegisterServices(hostServices));
        }

        public Task<QueryServiceResponse> GetServiceEntry(string name)
            => GlobalResolver.Ask<QueryServiceResponse>(new QueryServiceRequest(name), TimeSpan.FromSeconds(10));
    }
}