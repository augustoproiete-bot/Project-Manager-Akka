using Akka.Actor;
using Akka.Routing;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.Dispatcher
{
    [PublicAPI]
    public sealed class ConcurentDispatcher : IDispatcher
    {
        public Props Configurate(Props mutator)
            => mutator.WithRouter(
                new SmallestMailboxPool(2,
                    new DefaultResizer(2, 10),
                    Pool.DefaultSupervisorStrategy,
                    mutator.Dispatcher));
    }
}