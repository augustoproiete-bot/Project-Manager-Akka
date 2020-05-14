using System;

namespace Tauron.Application.Localizer.UIModels.Services.Data.MutatingEngine
{
    public interface IDataSource<TData>
    {
        TData GetData();

        void SetData(TData data);
    }
}