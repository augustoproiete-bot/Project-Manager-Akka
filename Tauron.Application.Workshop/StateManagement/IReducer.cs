using System.Threading.Tasks;
using Functional.Maybe;
using Tauron.Application.Workshop.Mutating;

namespace Tauron.Application.Workshop.StateManagement
{
    public interface IReducer<TData>
    {
        Task<Maybe<ReducerResult<TData>>> Reduce(Maybe<MutatingContext<TData>> state, IStateAction action);

        bool ShouldReduceStateForAction(IStateAction action);
    }
}