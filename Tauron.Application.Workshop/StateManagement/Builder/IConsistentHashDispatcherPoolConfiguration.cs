using Akka.Routing;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    [PublicAPI]
    public interface IConsistentHashDispatcherPoolConfiguration : IDispatcherPoolConfiguration<IConsistentHashDispatcherPoolConfiguration>
    {
        public IConsistentHashDispatcherPoolConfiguration WithVirtualNodesFactor(int vnodes);
    }
}