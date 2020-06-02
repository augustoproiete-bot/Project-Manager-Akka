using Akka.Code.Configuration;
using JetBrains.Annotations;

namespace Tauron.Application.Akka.ServiceResolver.Configuration
{
    [PublicAPI]
    public static class SrExtensions
    {
        public static ServiceResolverConfiguration ServiceResolver(this AkkaRootConfiguration config)
        {
            return config.Akka.ElementAcessor.GetAddElement<ServiceResolverConfiguration>("ServiceResolver");
        }
    }
}