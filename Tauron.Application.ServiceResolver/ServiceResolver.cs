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
        public ServiceResolver(ExtendedActorSystem system)
        {
        }

        public static ServiceResolver Get(ActorSystem system)
            => system.WithExtension<ServiceResolver, ServiceResolverProvider>();
    }
}