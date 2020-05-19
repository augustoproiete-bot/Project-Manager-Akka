namespace Tauron.Application.Localizer.DataModel.Workspace.MutatingEngine
{
    public interface IDataSource<TData>
    {
        TData GetData();

        void SetData(TData data);
    }
}