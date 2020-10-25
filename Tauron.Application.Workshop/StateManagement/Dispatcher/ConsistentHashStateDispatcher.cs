using Akka.Actor;
using Akka.Routing;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.Dispatcher
{
    [PublicAPI]
    public class ConsistentHashStateDispatcher : IStateDispatcherConfigurator
    {
        public Props Configurate(Props mutator) 
            => mutator.WithRouter(
                new ConsistentHashingPool(2)
                   .WithResizer(new DefaultResizer(2, 10)));
    }
}