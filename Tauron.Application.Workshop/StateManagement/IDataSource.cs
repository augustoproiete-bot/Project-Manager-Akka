using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement
{
    public interface IStateDataSource<TData> : IDataSource<TData>
    {
        void Apply(string query);
    }
}