using Akka.Actor;
using Akka.Routing;
using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    public sealed class ConcurrentDispatcherConfugiration : DispatcherPoolConfigurationBase<IConcurrentDispatcherConfugiration>, IConcurrentDispatcherConfugiration
    {
        public override IStateDispatcherConfigurator Create() => new ActualDispatcher(Instances, Resizer, SupervisorStrategy, Dispatcher);

        private sealed class ActualDispatcher : IStateDispatcherConfigurator
        {
            private readonly int _instances;
            private readonly Resizer? _resizer;
            private readonly SupervisorStrategy _supervisorStrategy;
            private readonly string? _dispatcher;

            public ActualDispatcher(int instances, Resizer? resizer, SupervisorStrategy supervisorStrategy, string? dispatcher)
            {
                _instances = instances;
                _resizer = resizer;
                _supervisorStrategy = supervisorStrategy;
                _dispatcher = dispatcher;
            }

            public Props Configurate(Props mutator)
            {
                var route = new SmallestMailboxPool(_instances)
                   .WithSupervisorStrategy(_supervisorStrategy);

                if (_resizer == null)
                    route = route.WithResizer(_resizer);
                if (!string.IsNullOrWhiteSpace(_dispatcher))
                    route = route.WithDispatcher(_dispatcher);

                return mutator.WithRouter(route);
            }
        }
    }
}