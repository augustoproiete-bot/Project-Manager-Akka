using JetBrains.Annotations;

namespace Akka.Code.Configuration.Elements
{
    [PublicAPI]
    public sealed class SerializationBindingsConfiguration : ConfigurationElement
    {
        public string this[string name]
        {
            get => Get<string>(name);
            set => Set(value, name);
        }
    }
}