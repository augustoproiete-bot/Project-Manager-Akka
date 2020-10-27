using Akka.Routing;

namespace Tauron.Application.Workshop.StateManagement
{
    public interface IStateAction
    {
        string ActionName { get; }

        string Query { get; }
    }
}