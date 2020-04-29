using System.Collections.Generic;
using Akka.Actor;

namespace Tauron.Application.Akka.ServiceResolver.Core
{
    public sealed class ResolverExt : IExtension
    {
        internal Dictionary<string, IActorRef> Services = new Dictionary<string, IActorRef>();

        public IActorRef HostActor { get; }
    }
}