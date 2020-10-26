namespace Tauron.Application.Workshop.StateManagement
{
    public interface IStateAction<TData>
    {
        string Query { get; }
    }
}