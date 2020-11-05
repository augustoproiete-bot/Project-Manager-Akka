using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.ServiceManager.Core.Managment.Data
{
    public sealed class ClusterConfiguration : IStateEntity
    {
        public bool IsDeleted => false;
        public string Id => nameof(ClusterConfiguration);


    }
}