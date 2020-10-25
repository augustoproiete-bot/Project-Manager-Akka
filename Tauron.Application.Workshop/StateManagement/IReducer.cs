namespace Tauron.Application.Workshop.StateManagement
{
    public interface IReducer<TData>
    {
        TData Reduce(TData state, object action);

        bool ShouldReduceStateForAction(object action);
    }
}