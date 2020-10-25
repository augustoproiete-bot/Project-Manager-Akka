using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.Dispatcher
{
    [PublicAPI]
    public sealed class DefaultStateDispatcher : IStateDispatcherConfigurator
    {
        public Props Configurate(Props mutator) => mutator;
    }
}