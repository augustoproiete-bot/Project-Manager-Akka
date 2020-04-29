using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Akka.ServiceResolver.Core;

namespace Tauron.Application.Akka.ServiceResolver
{
    [PublicAPI]
    public static class Extension
    {
        public static ResolverExt AddServiceResolver(this ActorSystem system) 
            => (ResolverExt) system.RegisterExtension(ResolverExtension.Id);
    }
}