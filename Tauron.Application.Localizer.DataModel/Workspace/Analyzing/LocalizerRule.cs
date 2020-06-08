using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Actor.Dsl;
using Tauron.Akka;
using Tauron.Application.Workshop;
using Tauron.Application.Workshop.Analyzing;
using Tauron.Application.Workshop.Analyzing.Rules;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Localizer.DataModel.Workspace.Analyzing
{
    public abstract class LocalizerRule : RuleBase<ProjectFileWorkspace, MutatingContext<ProjectFile>>
    {
        public ExposedReceiveActor Actor { get; private set; } = null!;

        protected override void ActorConstruct(ExposedReceiveActor actor)
        {
            Actor = actor;
            actor.RespondOnEventSource(Workspace.Source.ProjectReset, rest => SendIssues(ValidateAll(rest, actor.ExposedContext), actor.ExposedContext));
            RegisterRespond(actor);
        }

        protected abstract IEnumerable<Issue.IssueCompleter> ValidateAll(ProjectRest projectRest, IActorContext context);

        protected abstract void RegisterRespond(ExposedReceiveActor actor);

        protected void RegisterRespond<TData>(IEventSource<TData> source, Func<TData, IEnumerable<Issue.IssueCompleter>> validator) 
            => Actor.RespondOnEventSource(source, data => SendIssues(validator(data), Actor.ExposedContext));
    }
}