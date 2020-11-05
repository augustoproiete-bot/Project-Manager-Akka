using System;
using System.Collections.Concurrent;
using System.Linq;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.DataFactorys
{
    [PublicAPI]
    public sealed partial class MergeFactory : AdvancedDataSourceFactory
    {
        private ConcurrentBag<AdvancedDataSourceFactory> _factories = new ConcurrentBag<AdvancedDataSourceFactory>();

        public void Register(AdvancedDataSourceFactory factory) 
            => _factories.Add(factory);

        public override bool CanSupply(Type dataType) => _factories.Any(f => f.CanSupply(dataType));

        public override Func<IExtendedDataSource<TData>> Create<TData>() => _factories.First(f => f.CanSupply(typeof(TData))).Create<TData>();
    }
}