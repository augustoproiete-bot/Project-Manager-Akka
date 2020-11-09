using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.ServiceManager.Core.Managment.Events
{
    public sealed class ConfigurationChangedEvent : MutatingChange
    {
        public string NewConfig { get; }

        public ConfigurationChangedEvent(string newConfig) => NewConfig = newConfig;
    }
}