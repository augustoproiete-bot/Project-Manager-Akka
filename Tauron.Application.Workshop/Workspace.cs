using JetBrains.Annotations;
using Tauron.Application.Workshop.Analyzing;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutating.Changes;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.Workshop
{
    [PublicAPI]
    public abstract class WorkspaceBase<TData> : IDataSource<TData>, IState<TData>
        where TData : class
    {
        protected WorkspaceBase(WorkspaceSuperviser superviser)
        {
            Engine = MutatingEngine.From(this, superviser);
        }

        public void Dispatch(IDataMutation mutationOld)
            => Engine.Mutate(mutationOld);

        protected MutatingEngine<TData> Engine { get; }

        TData IDataSource<TData>.GetData() 
            => GetDataInternal();

        void IDataSource<TData>.SetData(TData data) 
            => SetDataInternal(data);

        protected abstract TData GetDataInternal();

        protected abstract void SetDataInternal(TData data);
    }

    [PublicAPI]
    public abstract class Workspace<TThis, TRawData> : WorkspaceBase<MutatingContext<TRawData>>
        where TThis : Workspace<TThis, TRawData>

    {
        protected Workspace(WorkspaceSuperviser superviser)
            : base(superviser) =>
            Analyzer = Analyzing.Analyzer.From<TThis, MutatingContext<TRawData>>((TThis) this, superviser);

        public IAnalyzer<TThis, MutatingContext<TRawData>> Analyzer { get; }

        public void Reset(TRawData newData) 
            => Engine.Mutate(nameof(Reset), data => data.Update(new ResetChange(), newData));
    }
}