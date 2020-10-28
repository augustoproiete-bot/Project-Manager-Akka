using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public abstract class StateBase<TData> : IState<TData>
    {
        public IEventSource<TData> OnChange { get; }

        protected StateBase(MutatingEngine<MutatingContext<TData>> engine)
        {
            OnChange = engine.EventSource(c => c.Data);
        }
    }
}