using JetBrains.Annotations;

namespace Akka.Code.Configuration.Elements
{
    [PublicAPI]
    public sealed class RemoteConfiguration : ConfigurationElement
    {
        public DotNettyTcpConfiguration AddDotNettyTcp()
        {
            return GetAddElement<DotNettyTcpConfiguration>("dot-netty.tcp");
        }
    }
}