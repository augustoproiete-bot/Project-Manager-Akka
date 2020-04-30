using Tauron.Application.Akka.ServiceResolver.Core;

namespace Tauron.Application.Akka.ServiceResolver.Actor
{
    public class GlobalResolver : ResolveActorBase
    {
        private readonly ResolverExt _ext;

        public GlobalResolver(ResolverExt ext)
        {
            _ext = ext;
        }
    }
}