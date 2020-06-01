namespace Tauron.Application.Workshop.Mutation
{
    public interface IDataSource<TData>
    {
        TData GetData();

        void SetData(TData data);
    }
}