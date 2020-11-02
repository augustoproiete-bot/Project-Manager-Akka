using System;

namespace Tauron.Application.Workshop.StateManagement
{
    public interface IDataSourceFactory
    {
        Func<IStateDataSource<TData>> Create<TData>()
            where TData : class, IStateEntity;
    }
}