using System.Threading.Tasks;

namespace Tauron.Application.Workshop.Mutation
{
    public interface IExtendedDataSource<TData>
    {
        Task<TData> GetData(IQuery query);

        Task SetData(IQuery query, TData data);

        Task OnCompled(IQuery query);
    }
}