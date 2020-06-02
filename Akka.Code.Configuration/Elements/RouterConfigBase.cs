using JetBrains.Annotations;

namespace Akka.Code.Configuration.Elements
{
    [PublicAPI]
    public abstract class RouterConfigBase : ConfigurationElement
    {
        protected RouterConfigBase(AkkaType type)
        {
            Router = type;
        }

        public AkkaType? Router
        {
            get => Get<AkkaType>("router");
            private set => Set(value, "router");
        }
    }
}