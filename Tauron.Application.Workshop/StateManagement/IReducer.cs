using Tauron.Application.Workshop.Mutating;

namespace Tauron.Application.Workshop.StateManagement
{
    public interface IReducer<TData>
    {
        ReducerResult<TData> Reduce(MutatingContext<TData> state, IStateAction action);

        bool ShouldReduceStateForAction(IStateAction action);
    }
}