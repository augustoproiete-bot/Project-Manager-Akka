using JetBrains.Annotations;

namespace Akka.Code.Configuration.Elements
{
    [PublicAPI]
    public class DispatcherConfiguration : ConfigurationElement
    {
        public AkkaType? Type
        {
            get => Get<AkkaType>("type");
            set => Set(value, "type");
        }

        public AkkaType? Executor
        {
            get => Get<AkkaType>("executor");
            set => Set(value, "executor");
        }

        public int Throughput
        {
            get => Get<int>("throughput");
            set => Set(value, "throughput");
        }
    }
}