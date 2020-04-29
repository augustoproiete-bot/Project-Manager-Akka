using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Akka.ServiceResolver.Actor;

namespace Tauron.Application.Akka.ServiceResolver.Core
{
    [PublicAPI]
    public sealed class ResolverExt : IExtension
    {
        internal ConcurrentDictionary<string, IActorRef> Services = new ConcurrentDictionary<string, IActorRef>();

        public IActorRef GlobalResolver { get; } 

        public IActorRef HostActor { get; }

        public IActorRef ProviderActor { get; }

        public ResolverExt(ExtendedActorSystem system)
        {
            GlobalResolver = system.ActorOf(Props.Create(() => new GlobalResolverActor(this)));
            HostActor = system.ActorOf(Props.Create(() => new HostActor(this)));
            ProviderActor = system.ActorOf(Props.Create(() => new ProviderActor(this)));
        }
    }
}