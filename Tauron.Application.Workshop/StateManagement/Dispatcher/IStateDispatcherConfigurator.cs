using Akka.Actor;

namespace Tauron.Application.Workshop.StateManagement.Dispatcher
{
    public interface IStateDispatcherConfigurator
    {
        Props Configurate(Props mutator);
    }
}