using Tauron.Application.ServiceManager.Core.Configuration;
using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.ServiceManager.Core.Managment.Data
{
    public sealed class ClusterConfiguration : IStateEntity
    {
        bool IStateEntity.IsDeleted => false;
        string IStateEntity.Id => nameof(ClusterConfiguration);

        public AppConfig Config { get; }

        public ClusterConfiguration(AppConfig config) 
            => Config = config;
    }
}