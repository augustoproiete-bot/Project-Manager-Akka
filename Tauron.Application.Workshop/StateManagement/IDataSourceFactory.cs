using System;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement
{
    public interface IDataSourceFactory
    {
        Func<IExtendedDataSource<TData>> Create<TData>()
            where TData : class, IStateEntity;
    }
}