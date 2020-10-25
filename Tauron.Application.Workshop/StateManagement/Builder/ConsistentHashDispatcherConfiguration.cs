using Akka.Actor;
using Akka.Routing;
using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    public sealed class ConsistentHashDispatcherConfiguration : DispatcherPoolConfigurationBase<IConsistentHashDispatcherPoolConfiguration>, IConsistentHashDispatcherPoolConfiguration
    {
        private int? _vNotes = null;

        private ConsistentHashMapping? _mapping;

        public override IStateDispatcherConfigurator Create() 
            => new ActualDispatcher(Instances, Resizer, SupervisorStrategy, Dispatcher, _mapping, _vNotes);

        public IConsistentHashDispatcherPoolConfiguration WithVirtualNodesFactor(int vnodes)
        {
            _vNotes = vnodes;
            return this;
        }

        public IConsistentHashDispatcherPoolConfiguration WithHashMapping(ConsistentHashMapping mapping)
        {
            _mapping = mapping;
            return this;
        }

        private sealed class ActualDispatcher : IStateDispatcherConfigurator
        {
            private readonly int _instances;
            private readonly Resizer? _resizer;
            private readonly SupervisorStrategy _supervisorStrategy;
            private readonly string? _dispatcher;
            private readonly ConsistentHashMapping? _mapping;
            private readonly int? _vNotes;

            public ActualDispatcher(int instances, Resizer? resizer, SupervisorStrategy supervisorStrategy, string? dispatcher, ConsistentHashMapping? mapping, int? vNotes)
            {
                _instances = instances;
                _resizer = resizer;
                _supervisorStrategy = supervisorStrategy;
                _dispatcher = dispatcher;
                _mapping = mapping;
                _vNotes = vNotes;
            }

            public Props Configurate(Props mutator)
            {
                var router = new ConsistentHashingPool(_instances)
                   .WithSupervisorStrategy(_supervisorStrategy)
                   .WithHashMapping(_mapping);

                if (_resizer != null)
                    router = router.WithResizer(_resizer);
                if (!string.IsNullOrWhiteSpace(_dispatcher))
                    router = router.WithDispatcher(_dispatcher);
                if (_vNotes != null)
                    router = router.WithVirtualNodesFactor(_vNotes.Value);

                return mutator.WithRouter(router);
            }
        }
    }
}