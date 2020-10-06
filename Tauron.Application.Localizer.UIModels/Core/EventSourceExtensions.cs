using System;
using Akka.Actor;
using Tauron.Application.Localizer.DataModel.Workspace;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels.Core
{
    public static class EventSourceExtensions
    {
        public static CommandQuery FromEventSource<TData>(this CommandQueryBuilder builder, IEventSource<TData> source, Func<TData, bool> check, TData defaultValue)
            => builder.FromExternal(check, action => source.RespondOn(ActorCell.GetCurrentSelfOrNoSender() ?? ActorRefs.Nobody, action), defaultValue);

        public static CommandQuery FromEventSource<TData>(this CommandQueryBuilder builder, IEventSource<TData> source, Func<TData, bool> check)
            where TData : class
            => builder.FromExternal<TData>(d => d != null && check(d), action => source.RespondOn(ActorCell.GetCurrentSelfOrNoSender() ?? ActorRefs.Nobody, action));

        public static CommandQuery NoEmptyProjectFile(this CommandQueryBuilder builder, ProjectFileWorkspace workspace)
            => builder.FromEventSource(workspace.Source.ProjectReset, rest => !rest.ProjectFile.IsEmpty, new ProjectRest(workspace.ProjectFile));
    }
}