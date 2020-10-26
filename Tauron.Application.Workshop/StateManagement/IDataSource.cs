using Tauron.Application.Workshop.StateManagement.Internal;

namespace Tauron.Application.Workshop.StateManagement
{
    public interface IStateDataSource<TData> : IStateDataSource
    {
        TData Get(string query);

        void Set(string query, TData data);
    }
}