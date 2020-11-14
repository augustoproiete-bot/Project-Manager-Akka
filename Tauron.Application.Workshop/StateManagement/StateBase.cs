using System.Threading.Tasks;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public abstract class StateBase<TData> : IState<TData>, ICanQuery<TData> where TData : class
    {
        private IExtendedDataSource<MutatingContext<TData>>? _source;

        public IEventSource<TData> OnChange { get; }

        protected StateBase(ExtendedMutatingEngine<MutatingContext<TData>> engine) 
            => OnChange = engine.EventSource(mayc => mayc.Select(c => c.Data));

        void ICanQuery<TData>.DataSource(IExtendedDataSource<MutatingContext<TData>> source) 
            => _source = source;

        public async Task<Maybe<TData>> Query(IQuery query)
        {
            var source = _source;
            return source == null ? Maybe<TData>.Nothing : (await source.GetData(query)).Select(c => c.Data);
        }
    }
}