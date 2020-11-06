using Akka.Actor;
using Tauron.Application.ServiceManager.Core.Configuration;
using Tauron.Application.Workshop.StateManagement.Attributes;
using Tauron.Application.Workshop.StateManagement.DataFactorys;

namespace Tauron.Application.ServiceManager.Core.Managment.Data
{
    [DataSource]
    public sealed class ClusterConfigurationSource : SingleValueDataFactory<ClusterConfiguration>
    {
        private readonly AppConfig _config;
        private readonly ActorSystem _system;

        public ClusterConfigurationSource(AppConfig config, ActorSystem system)
        {
            _config = config;
            _system = system;
        }

        protected override ClusterConfiguration CreateValue() => new ClusterConfiguration(_config, _system);
    }
}