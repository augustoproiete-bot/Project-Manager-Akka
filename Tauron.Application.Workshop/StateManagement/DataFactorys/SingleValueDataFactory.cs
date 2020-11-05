using System;
using System.Threading;
using System.Threading.Tasks;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.DataFactorys
{
    public abstract class SingleValueDataFactory<TData> : AdvancedDataSourceFactory
        where TData : IStateEntity
    {
        private readonly Lazy<object> _lazyData;

        protected SingleValueDataFactory() 
            => _lazyData = new Lazy<object>(() => new SingleValueSource(CreateValue()), LazyThreadSafetyMode.ExecutionAndPublication);

        public override bool CanSupply(Type dataType) => dataType == typeof(TData);

        public override Func<IExtendedDataSource<TRealData>> Create<TRealData>() 
            => () => (IExtendedDataSource<TRealData>) _lazyData.Value;

        protected abstract TData CreateValue();

        private sealed class SingleValueSource : IExtendedDataSource<TData>, IDisposable
        {
            private TData _value;
            private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0, 1);

            public SingleValueSource(TData value) => _value = value;

            public async Task<TData> GetData(IQuery query)
            {
                await _semaphore.WaitAsync();
                return _value;
            }

            public Task SetData(IQuery query, TData data)
            {
                _value = data;
                return Task.CompletedTask;
            }

            public Task OnCompled(IQuery query)
            {
                _semaphore.Release();
                return Task.CompletedTask;
            }

            public void Dispose() => _semaphore.Dispose();
        }
    }
}