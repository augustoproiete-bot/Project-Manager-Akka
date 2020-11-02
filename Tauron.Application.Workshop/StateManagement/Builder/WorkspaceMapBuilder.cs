using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Autofac;
using CacheManager.Core;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Internal;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    public class WorkspaceMapBuilder<TData> : StateBuilderBase, IWorkspaceMapBuilder<TData>
        where TData : class
    {
        private readonly Func<WorkspaceBase<TData>> _workspace;
        private readonly Dictionary<Type, Func<WorkspaceBase<TData>, IStateAction, IDataMutation>> _map = new Dictionary<Type, Func<WorkspaceBase<TData>, IStateAction, IDataMutation>>();

        public WorkspaceMapBuilder(Func<WorkspaceBase<TData>> workspace) 
            => _workspace = workspace;

        public override (StateContainer State, string Key) Materialize(MutatingEngine engine, ICache<object?>? parent, IComponentContext? componentContext) 
            => (new WorkspaceContainer<TData>(_map.ToImmutableDictionary(), _workspace()), string.Empty);

        public IWorkspaceMapBuilder<TData> MapAction<TAction>(Func<WorkspaceBase<TData>, IDataMutation> from)
        {
            _map[typeof(TAction)] = (work, action) => from(work);
            return this;
        }

        public IWorkspaceMapBuilder<TData> MapAction<TAction>(Func<WorkspaceBase<TData>, TAction, IDataMutation> from)
        {
            _map[typeof(TAction)] = (work, action) => from(work, (TAction) action);
            return this;
        }
    }
}