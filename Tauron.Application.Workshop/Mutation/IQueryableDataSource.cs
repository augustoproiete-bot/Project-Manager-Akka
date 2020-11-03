using System.Threading.Tasks;

namespace Tauron.Application.Workshop.Mutation
{
    public interface IQueryableDataSource<TData>
    {
        Task<TData> GetData(IQuery query);

        Task SetData(IQuery query, TData data);
    }
}