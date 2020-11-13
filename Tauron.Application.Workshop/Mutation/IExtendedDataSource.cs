using System.Threading.Tasks;
using Functional.Maybe;

namespace Tauron.Application.Workshop.Mutation
{
    public interface IExtendedDataSource<TData>
    {
        Task<Maybe<TData>> GetData(IQuery query);

        Task SetData(IQuery query, Maybe<TData> data);

        Task OnCompled(IQuery query);
    }
}