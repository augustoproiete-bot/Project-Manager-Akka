using System;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    public interface IWorkspaceMapBuilder<TData> where TData : class
    {
        IWorkspaceMapBuilder<TData> MapAction<TAction>(Func<WorkspaceBase<TData>, IDataMutation> from);

        IWorkspaceMapBuilder<TData> MapAction<TAction>(Func<WorkspaceBase<TData>, TAction, IDataMutation> from);
    }
}