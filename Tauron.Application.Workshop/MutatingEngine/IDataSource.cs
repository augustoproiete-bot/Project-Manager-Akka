namespace Tauron.Application.Workshop.MutatingEngine
{
    public interface IDataSource<TData>
    {
        TData GetData();

        void SetData(TData data);
    }
}