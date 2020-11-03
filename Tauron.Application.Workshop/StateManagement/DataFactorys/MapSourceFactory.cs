using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.DataFactorys
{
    [PublicAPI]
    public class MapSourceFactory : AdvancedDataSourceFactory
    {
        public ConcurrentDictionary<Type, Func<object>> Map { get; private set; } = new ConcurrentDictionary<Type, Func<object>>();

        public void Register<TSource, TData>(Func<TSource> factory)
            where TSource : IQueryableDataSource<TData>
        {
            var creator = new Func<object>(() => factory());

            Map.AddOrUpdate(typeof(TSource), () => creator, (key, oldValue) => creator);
        }

        public override bool CanSupply(Type dataType) => Map.ContainsKey(dataType);

        public override Func<IQueryableDataSource<TData>> Create<TData>()
        {
            if (Map.TryGetValue(typeof(TData), out var fac))
                return () => (IQueryableDataSource<TData>) fac();

            throw new InvalidOperationException("Not Supported Data Type Mapping");
        }
    }
}