using System.Threading.Tasks;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public abstract class StateBase<TData> : IState<TData>, ICanQuery<TData> where TData : class, IStateEntity
    {
        private IQueryableDataSource<MutatingContext<TData>>? _source;

        public IEventSource<TData> OnChange { get; }

        protected StateBase(QueryableMutatingEngine<MutatingContext<TData>> engine)
        {
            OnChange = engine.EventSource(c => c.Data);
        }

        void ICanQuery<TData>.DataSource(IQueryableDataSource<MutatingContext<TData>> source) => _source = source;

        public Task<TData?> Query(IQuery query)
        {
            return Task.Run(() =>
            {
                var source = _source;
                return source?.GetData(query).Data;
            });
        }
    }
}