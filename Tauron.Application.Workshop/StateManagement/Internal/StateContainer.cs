using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheManager.Core;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.Internal
{
    public abstract class StateContainer : IDisposable
    {
        public IState Instance { get; }

        protected StateContainer(IState instance) 
            => Instance = instance;

        public abstract void TryDipatch<TActionData>(IStateAction<TActionData> action);
        public abstract void Dispose();
    }

    public sealed class StateContainer<TData> : StateContainer
        where TData : class
    {
        private readonly MutatingEngine<TData> _engine;

        private string CacheKey = string.Empty;

        private IReadOnlyCollection<IReducer<TData>> Reducers { get; }

        private ICache<TData>? Cache { get; }

        private IStateDataSource<TData> DataSource { get; }

        public StateContainer(IState instance, IReadOnlyCollection<IReducer<TData>> reducers, ICache<TData>? cache, IStateDataSource<TData> dataSource, MutatingEngine<TData> engine) 
            : base(instance)
        {
            _engine = engine;
            Reducers = reducers;
            Cache = cache;
            DataSource = dataSource;

            if(cache == null)
                return;
            CacheKey = $"--{instance.GetType().Name}--{Guid.NewGuid():N}";
        }

        public override void TryDipatch<TActionData>(IStateAction<TActionData> action)
        {
            TData data;
            if (Cache != null)
                data = Cache.Get(action.Query, CacheKey) ?? DataSource.Get(action.Query);
            else
                data = DataSource.Get(action.Query);
            var isProcessed = false;

            data = Reducers.Where(r => r.ShouldReduceStateForAction(action)).Aggregate(data, (input, reducer) =>
            {
                isProcessed = true;
                return reducer.Reduce(input, action);
            });

            if (!isProcessed)
                return false;

            Cache?.Put(action.Query, data, CacheKey);
            DataSource.Set(action.Query, data);

            return true;
        }

        public override void Dispose() 
            => Cache?.Dispose();
    }
}