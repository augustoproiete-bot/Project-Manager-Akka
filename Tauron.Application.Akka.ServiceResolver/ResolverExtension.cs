using Akka.Actor;
using Tauron.Application.Akka.ServiceResolver.Core;

namespace Tauron.Application.Akka.ServiceResolver
{
    public sealed class ResolverExtension : ExtensionIdProvider<ResolverExt>
    {
        public static readonly ResolverExtension Id = new ResolverExtension();

        public override ResolverExt CreateExtension(ExtendedActorSystem system)
        {
            var ext = new ResolverExt(system);
            ext.Initialize();
            return ext;
        }
    }
}