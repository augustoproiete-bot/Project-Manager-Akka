namespace Tauron.Application.Workshop.StateManagement
{
    public interface IReducer<TData>
    {
        TData Reduce(TData state, IStateAction<TData> action);

        bool ShouldReduceStateForAction(IStateAction<TData> action);
    }
}