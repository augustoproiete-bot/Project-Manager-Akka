using System;
using Tauron.Application.Localizer.DataModel.Workspace;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels.Core
{
    public static class EventSourceExtensions
    {
        public static CommandQuery FromEventSource<TData>(this CommandQueryBuilder builder, IEventSource<TData> source, Func<TData, bool> check, TData defaultValue)
            => builder.FromExternal(check, source.RespondOn, defaultValue);

        public static CommandQuery FromEventSource<TData>(this CommandQueryBuilder builder, IEventSource<TData> source, Func<TData, bool> check)
            where TData : class
            => builder.FromExternal<TData>(d => d != null && check(d), source.RespondOn);

        public static CommandQuery NoEmptyProjectFile(this CommandQueryBuilder builder, ProjectFileWorkspace workspace)
            => builder.FromEventSource(workspace.Source.ProjectReset, rest => !rest.ProjectFile.IsEmpty, new ProjectRest(workspace.ProjectFile));
    }
}