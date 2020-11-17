using System;
using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.DataFactorys
{
    public abstract class SingleValueDataFactory<TData> : AdvancedDataSourceFactory
    {
        private readonly SemaphoreSlim _sync = new(1);
        private int _sourceCount;

        public override bool CanSupply(Type dataType) => dataType == typeof(TData);

        public override Func<IExtendedDataSource<TRealData>> Create<TRealData>()
        {
            ThrowDispose();
            return () =>
                   {
                       ThrowDispose();
                       Interlocked.Increment(ref _sourceCount);
                       return new SingleValueSource(CreateValue, UpdateFrom, ContextDisposed, _sync) as IExtendedDataSource<TRealData> ?? 
                              throw new InvalidCastException("Data Type not Compatiple");
                   };
        }

        private void ContextDisposed()
        {
            if(Interlocked.Decrement(ref _sourceCount) == 0)
                Dispose();
        }

        protected abstract Task<Maybe<TData>> CreateValue();

        protected abstract Task UpdateFrom(Maybe<TData> data);

        protected override void DisposeCore(bool disposing)
        {
            if(disposing)
                _sync.Dispose();
            base.DisposeCore(disposing);
        }

        private sealed class SingleValueSource : DisposeableBase, IExtendedDataSource<TData>
        {
            private readonly Func<Task<Maybe<TData>>> _createValue;
            private readonly Func<Maybe<TData>, Task> _update;
            private readonly Action                   _onDispose;
            private readonly SemaphoreSlim            _sync;

            public SingleValueSource(Func<Task<Maybe<TData>>> createValue, Func<Maybe<TData>, Task> update, Action onDispose, SemaphoreSlim sync)
            {
                _createValue = createValue;
                _update      = update;
                _onDispose   = onDispose;
                _sync   = sync;
            }

            public async Task<Maybe<TData>> GetData(IQuery query)
            {
                ThrowDispose();
                await _sync.WaitAsync();
                return await _createValue();
            }

            public async Task SetData(IQuery query, Maybe<TData> data)
            {
                ThrowDispose();
                await _update(data);
            }

            public Task OnCompled(IQuery query)
            {
                ThrowDispose();
                _sync.Release();
                return Task.CompletedTask;
            }

            protected override void DisposeCore(bool disposing) => _onDispose();
        }
    }
}