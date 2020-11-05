using Tauron.Application.ServiceManager.Core.Configuration;
using Tauron.Application.Workshop.StateManagement.Attributes;
using Tauron.Application.Workshop.StateManagement.DataFactorys;

namespace Tauron.Application.ServiceManager.Core.Managment.Data
{
    [DataSource]
    public sealed class ClusterConfigurationSource : SingleValueDataFactory<ClusterConfiguration>
    {
        private readonly AppConfig _config;

        public ClusterConfigurationSource(AppConfig config) => _config = config;

        protected override ClusterConfiguration CreateValue() => new ClusterConfiguration(_config);
    }
}