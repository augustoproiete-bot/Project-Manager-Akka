using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement
{
    public interface IState
    {
        
    }

    public interface IState<TData>
    {
        void Initialize(MutatingEngine<TData> engine);


    }
}