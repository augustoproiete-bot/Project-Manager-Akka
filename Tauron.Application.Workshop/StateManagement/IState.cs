using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement
{
    public interface IState
    {
        
    }

    public interface IState<TData> : IState
    {
    }
}