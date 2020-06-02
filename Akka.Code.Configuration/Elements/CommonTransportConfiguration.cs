using JetBrains.Annotations;

namespace Akka.Code.Configuration.Elements
{
    [PublicAPI]
    public abstract class CommonTransportConfiguration : ConfigurationElement
    {
        public string HostName
        {
            get => Get<string>("hostname");
            set => Set(value, "hostname");
        }

        public int Port
        {
            get => Get<int>("port");
            set => Set(value, "port");
        }
    }
}