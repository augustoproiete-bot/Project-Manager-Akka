using Akka.Actor;

namespace Tauron.Application.Workshop.StateManagement.Dispatcher
{
    public interface IDispatcher
    {
        Props Configurate(Props mutator);
    }
}