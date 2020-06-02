using Akka.Code.Configuration;
using JetBrains.Annotations;

namespace Tauron.Application.Akka.ServiceResolver.Configuration
{
    [PublicAPI]
    public sealed class ServiceResolverConfiguration : ConfigurationElement
    {
        public string Name
        {
            get => Get<string>(nameof(Name));
            set => Set(value, nameof(Name));
        }

        public string ResolverPath
        {
            get => Get<string>(nameof(ResolverPath));
            set => Set(value, nameof(ResolverPath));
        }

        public bool IsGlobal
        {
            get => Get<bool>(nameof(IsGlobal));
            set => Set(value, nameof(IsGlobal));
        }
    }
}