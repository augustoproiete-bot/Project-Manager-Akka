using System.Text;
using Akka.Code.Configuration.Elements;
using Akka.Configuration;
using JetBrains.Annotations;

namespace Akka.Code.Configuration
{
    [PublicAPI]
    public sealed class AkkaRootConfiguration : ConfigurationElement
    {
        public AkkaConfiguration Akka => GetAddElement<AkkaConfiguration>("akka");

        public Config CreateConfig()
        {
            var builder = new StringBuilder();
            Construct(builder);
            var value = builder.ToString();
            return string.IsNullOrWhiteSpace(value) ? Config.Empty : ConfigurationFactory.ParseString(value);
        }
    }
}