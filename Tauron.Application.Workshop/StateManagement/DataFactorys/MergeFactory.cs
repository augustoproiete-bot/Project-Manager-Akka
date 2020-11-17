using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Akka.Util.Internal;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.DataFactorys
{
    [PublicAPI]
    public sealed partial class MergeFactory : AdvancedDataSourceFactory
    {
        private readonly ConcurrentBag<AdvancedDataSourceFactory> _factories = new();

        public void Register(AdvancedDataSourceFactory factory)
        {
            if(factory is MergeFactory mergeFactory)
                RegisterMany(mergeFactory._factories);
            _factories.Add(factory);
        }

        public void RegisterMany(IEnumerable<AdvancedDataSourceFactory> factories)
            => factories.ForEach(f => _factories.Add(f));
        
        public override bool CanSupply(Type dataType)
        {
            ThrowDispose();
            return _factories.Any(f => f.CanSupply(dataType));
        }

        public override Func<IExtendedDataSource<TData>> Create<TData>()
        {
            ThrowDispose();
            return _factories.First(f => f.CanSupply(typeof(TData))).Create<TData>();
        }

        protected override void DisposeCore(bool disposing)
        {
            foreach (var disposable in _factories)
                disposable.Dispose();
            _factories.Clear();
            
            base.DisposeCore(disposing);
        }
    }
}