using System;

namespace Tauron.Application.Workshop.StateManagement.DataFactorys
{
    public abstract class AdvancedDataSourceFactory : IDataSourceFactory
    {
        public abstract bool CanSupply(Type dataType);

        public abstract Func<IStateDataSource<TData>> Create<TData>() where TData : class, IStateEntity;
    }
}