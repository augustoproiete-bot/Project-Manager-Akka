using System;
using Akka.Actor;
using Akka.Routing;
using Functional.Maybe;
using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    public sealed class ConsistentHashDispatcherConfiguration : DispatcherPoolConfigurationBase<IConsistentHashDispatcherPoolConfiguration>, IConsistentHashDispatcherPoolConfiguration
    {
        private Maybe<int> _vNotes;
        
        public override IStateDispatcherConfigurator Create() 
            => new ActualDispatcher(Instances, Resizer, SupervisorStrategy, Dispatcher, _vNotes, Custom);

        public IConsistentHashDispatcherPoolConfiguration WithVirtualNodesFactor(int vnodes)
        {
            _vNotes = vnodes.ToMaybe();
            return this;
        }

        private sealed class ActualDispatcher : IStateDispatcherConfigurator
        {
            private readonly int _instances;
            private readonly Maybe<Resizer> _resizer;
            private readonly SupervisorStrategy _supervisorStrategy;
            private readonly Maybe<string> _dispatcher;
            private readonly Maybe<int> _vNotes;
            private readonly Maybe<Func<Props, Props>> _custom;

            public ActualDispatcher(int instances, Maybe<Resizer> resizer, SupervisorStrategy supervisorStrategy, Maybe<string> dispatcher, Maybe<int> vNotes, Maybe<Func<Props, Props>> custom)
            {
                _instances = instances;
                _resizer = resizer;
                _supervisorStrategy = supervisorStrategy;
                _dispatcher = dispatcher;
                _vNotes = vNotes;
                _custom = custom;
            }

            public Props Configurate(Props mutator)
            {
                var router = new ConsistentHashingPool(_instances)
                    .WithSupervisorStrategy(_supervisorStrategy);

                if (_resizer.IsSomething())
                    router = router.WithResizer(_resizer.Value);
                if (_dispatcher.IsSomething() && !string.IsNullOrWhiteSpace(_dispatcher.Value))
                    router = router.WithDispatcher(_dispatcher.Value);
                if (_vNotes.IsSomething())
                    router = router.WithVirtualNodesFactor(_vNotes.Value);

                mutator = mutator.WithRouter(router);
                return _custom.IsSomething() ? _custom.Value(mutator) : mutator;
            }
        }
    }
}