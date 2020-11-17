using System;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.DataFactorys
{
    public abstract class AdvancedDataSourceFactory : DisposeableBase, IDataSourceFactory
    {
        public abstract bool CanSupply(Type dataType);

        public abstract Func<IExtendedDataSource<TData>> Create<TData>() where TData : class;

        protected override void DisposeCore(bool disposing)
        { }
    }
}