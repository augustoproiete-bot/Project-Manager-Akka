using System;
using JetBrains.Annotations;

namespace Akka.Code.Configuration.Elements
{
    [PublicAPI]
    public sealed class ActorConfuguration : ConfigurationElement
    {
        public DebugConfiguration Debug => GetAddElement<DebugConfiguration>("debug");

        public DeploymentConfiguration Deployment => GetAddElement<DeploymentConfiguration>("deployment");

        public SerializersConfiguration Serializers => GetAddElement<SerializersConfiguration>("serializers");

        public SerializationBindingsConfiguration SerializationBindings => GetAddElement<SerializationBindingsConfiguration>("serialization-bindings");

        public AkkaType? Provider
        {
            get => Get<AkkaType>("provider");
            set => Set(value, "provider");
        }

        public TimeSpan AskTimeout
        {
            get => Get<TimeSpan>("ask-timeout");
            set => Set(value, "ask-timeout");
        }

        public TConfig DefaultDispatcher<TConfig>()
            where TConfig : DispatcherConfiguration, new()
        {
            var c = new TConfig();
            Add("default-dispatcher", c);
            return c;
        }
    }
}