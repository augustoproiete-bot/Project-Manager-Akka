using System.Threading.Tasks;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public abstract class StateBase<TData> : IState<TData>, ICanQuery<TData> where TData : class, IStateEntity
    {
        private IExtendedDataSource<MutatingContext<TData>>? _source;

        public IEventSource<TData> OnChange { get; }

        protected StateBase(ExtendedMutatingEngine<MutatingContext<TData>> engine)
        {
            OnChange = engine.EventSource(c => c.Data);
        }

        void ICanQuery<TData>.DataSource(IExtendedDataSource<MutatingContext<TData>> source) => _source = source;

        public async Task<TData?> Query(IQuery query)
        {
            var source = _source;
            return source == null ? null : (await source.GetData(query)).Data;
        }
    }
}