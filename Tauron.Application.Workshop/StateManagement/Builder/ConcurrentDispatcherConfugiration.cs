using System;
using Akka.Actor;
using Akka.Routing;
using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    public sealed class ConcurrentDispatcherConfugiration : DispatcherPoolConfigurationBase<IConcurrentDispatcherConfugiration>, IConcurrentDispatcherConfugiration
    {
        public override IStateDispatcherConfigurator Create() => new ActualDispatcher(Instances, Resizer, SupervisorStrategy, Dispatcher, Custom);

        private sealed class ActualDispatcher : IStateDispatcherConfigurator
        {
            private readonly int _instances;
            private readonly Resizer? _resizer;
            private readonly SupervisorStrategy _supervisorStrategy;
            private readonly string? _dispatcher;
            private readonly Func<Props, Props>? _custom;

            public ActualDispatcher(int instances, Resizer? resizer, SupervisorStrategy supervisorStrategy, string? dispatcher, Func<Props, Props>? custom)
            {
                _instances = instances;
                _resizer = resizer;
                _supervisorStrategy = supervisorStrategy;
                _dispatcher = dispatcher;
                _custom = custom;
            }

            public Props Configurate(Props mutator)
            {
                var route = new SmallestMailboxPool(_instances)
                   .WithSupervisorStrategy(_supervisorStrategy);

                if (_resizer == null)
                    route = route.WithResizer(_resizer);
                if (!string.IsNullOrWhiteSpace(_dispatcher))
                    route = route.WithDispatcher(_dispatcher);
                
                mutator = mutator.WithRouter(route);
                return _custom != null ? _custom(mutator) : mutator;
            }
        }
    }
}