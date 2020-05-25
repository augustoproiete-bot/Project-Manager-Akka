using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Analyzing;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutating.Changes;
using Tauron.Application.Workshop.MutatingEngine;

namespace Tauron.Application.Workshop
{
    [PublicAPI]
    public abstract class WorkspaceBase<TData> : IDataSource<TData>
    {
        protected MutatingEngine<TData> Engine { get; }

        protected WorkspaceBase(IActorRefFactory factory) 
            => Engine = new MutatingEngine<TData>(factory, this);

        TData IDataSource<TData>.GetData() => GetDataInternal();

        protected abstract TData GetDataInternal();

        void IDataSource<TData>.SetData(TData data) => SetDataInternal(data);

        protected abstract void SetDataInternal(TData data);
    }

    [PublicAPI]
    public abstract class Workspace<TThis, TRawData> : WorkspaceBase<MutatingContext<TRawData>>
        where TThis : Workspace<TThis, TRawData>

    {
        public Analyzer<TThis, MutatingContext<TRawData>> Analyzer { get; }

        protected Workspace(IActorRefFactory factory)
            : base(factory) =>
            Analyzer = new Analyzer<TThis, MutatingContext<TRawData>>((TThis) this, factory);

        public void Reset(TRawData newData) 
            => Engine.Mutate(nameof(Reset), data => data.Update(new ResetChange(), newData));
    }
}