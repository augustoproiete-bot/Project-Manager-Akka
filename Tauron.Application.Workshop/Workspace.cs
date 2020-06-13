using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Analyzing;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutating.Changes;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop
{
    [PublicAPI]
    public abstract class WorkspaceBase<TData> : IDataSource<TData>
    {
        protected WorkspaceBase(IActorRefFactory factory)
        {
            Engine = MutatingEngine.From(this, factory);
        }

        protected MutatingEngine<TData> Engine { get; }

        TData IDataSource<TData>.GetData()
        {
            return GetDataInternal();
        }

        void IDataSource<TData>.SetData(TData data)
        {
            SetDataInternal(data);
        }

        protected abstract TData GetDataInternal();

        protected abstract void SetDataInternal(TData data);
    }

    [PublicAPI]
    public abstract class Workspace<TThis, TRawData> : WorkspaceBase<MutatingContext<TRawData>>
        where TThis : Workspace<TThis, TRawData>

    {
        protected Workspace(IActorRefFactory factory)
            : base(factory) =>
            Analyzer = Analyzing.Analyzer.From<TThis, MutatingContext<TRawData>>((TThis) this, factory);

        public IAnalyzer<TThis, MutatingContext<TRawData>> Analyzer { get; }

        public void Reset(TRawData newData) 
            => Engine.Mutate(nameof(Reset), data => data.Update(new ResetChange(), newData));
    }
}