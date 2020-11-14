using System;
using Akka.Actor;
using Akka.Routing;
using Functional.Maybe;
using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    public sealed class ConcurrentDispatcherConfugiration : DispatcherPoolConfigurationBase<IConcurrentDispatcherConfugiration>, IConcurrentDispatcherConfugiration
    {
        public override IStateDispatcherConfigurator Create() => new ActualDispatcher(Instances, Resizer, SupervisorStrategy, Dispatcher, Custom);

        private sealed class ActualDispatcher : IStateDispatcherConfigurator
        {
            private readonly int _instances;
            private readonly Maybe<Resizer> _resizer;
            private readonly SupervisorStrategy _supervisorStrategy;
            private readonly Maybe<string> _dispatcher;
            private readonly Maybe<Func<Props, Props>> _custom;

            public ActualDispatcher(int instances, Maybe<Resizer> resizer, SupervisorStrategy supervisorStrategy, Maybe<string> dispatcher, Maybe<Func<Props, Props>> custom)
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

                if (_resizer.IsSomething())
                    route = route.WithResizer(_resizer.Value);
                if (_dispatcher.IsSomething() && !string.IsNullOrWhiteSpace(_dispatcher.Value))
                    route = route.WithDispatcher(_dispatcher.Value);
                
                mutator = mutator.WithRouter(route);
                return _custom.IsSomething() ? _custom.Value(mutator) : mutator;
            }
        }
    }
}