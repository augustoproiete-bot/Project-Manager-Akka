using Akka.Routing;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement
{
    public interface IStateAction
    {
        string ActionName { get; }

        IQuery Query { get; }
    }
}