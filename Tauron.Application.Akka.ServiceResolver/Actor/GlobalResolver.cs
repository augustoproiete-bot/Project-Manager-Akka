using Tauron.Application.Akka.ServiceResolver.Core;
using Tauron.Application.Akka.ServiceResolver.Messages.Global;

namespace Tauron.Application.Akka.ServiceResolver.Actor
{
    public class GlobalResolver : ResolveActorBase
    {
        private readonly ResolverExt _ext;

        public GlobalResolver(ResolverExt ext)
        {
            _ext = ext;

            Receive<RegisterEndpointMessage>(RegisterEndpointMessage);
        }

        private void RegisterEndpointMessage(RegisterEndpointMessage obj)
        {
            _ext.
        }
    }
}