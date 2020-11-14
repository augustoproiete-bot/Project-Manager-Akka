using System.Threading.Tasks;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public interface ICanQuery<TData>
        where TData : class
    {
        void DataSource(IExtendedDataSource<MutatingContext<TData>> source);

        Task<Maybe<TData>> Query(IQuery query);
    }
}