using JetBrains.Annotations;

namespace Akka.Code.Configuration.Elements
{
    [PublicAPI]
    public sealed class SerializersConfiguration : ConfigurationElement
    {
        public AkkaType this[string name]
        {
            get => Get<AkkaType>(name);
            set => Set(value, name);
        }
    }
}