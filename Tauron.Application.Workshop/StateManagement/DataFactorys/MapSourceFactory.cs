using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.DataFactorys
{
    [PublicAPI]
    public class MapSourceFactory : AdvancedDataSourceFactory
    {
        public ConcurrentDictionary<Type, Func<object>> Map { get; private set; } = new ConcurrentDictionary<Type, Func<object>>();

        public void Register<TSource, TData>(Func<TSource> factory)
            where TSource : IStateDataSource<TData>
        {
            var creator = new Func<object>(() => factory());

            Map.AddOrUpdate(typeof(TSource), () => creator, (_, _) => creator);
        }

        public override bool CanSupply(Type dataType) => Map.ContainsKey(dataType);

        public override Func<IStateDataSource<TData>> Create<TData>()
        {
            if (Map.TryGetValue(typeof(TData), out var fac))
                return () => (IStateDataSource<TData>) fac();

            throw new InvalidOperationException("Not Supported Data Type Mapping");
        }
    }
}