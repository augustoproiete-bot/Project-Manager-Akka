using Functional.Maybe;

namespace Tauron.Application.Workshop.Mutation
{
    public interface IDataSource<TData>
    {
        Maybe<TData> GetData();

        void SetData(Maybe<TData> data);
    }
}