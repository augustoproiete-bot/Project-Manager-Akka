using System;
using System.Collections.Generic;
using Akka.Actor;
using Functional.Maybe;
using Tauron.Akka;
using Tauron.Application.Workshop;
using Tauron.Application.Workshop.Analyzing;
using Tauron.Application.Workshop.Analyzing.Rules;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using static Tauron.Prelude;

namespace Tauron.Application.Localizer.DataModel.Workspace.Analyzing
{
    public abstract class LocalizerRule : RuleBase<ProjectFileWorkspace, MutatingContext<ProjectFile>>
    {
        public Maybe<IExposedReceiveActor> Actor { get; private set; }

        protected override Maybe<Unit> ActorConstruct(Maybe<IExposedReceiveActor> mayActor)
        {
            void OnReset(Maybe<ProjectRest> pr)
            {
                Do(from reset in pr 
                   select Action(() => SendIssues(ValidateAll(reset, ExposedReceiveActor.ExposedContext), ExposedReceiveActor.ExposedContext)));
            }
            
            Actor = mayActor;

            return from workspace in Workspace
                   from actor in mayActor
                   from register in RegisterRespond(mayActor)
                   select Action(() => actor.RespondOnEventSource(workspace.Source.ProjectReset, OnReset));
        }

        protected abstract IEnumerable<Issue.IssueCompleter> ValidateAll(ProjectRest projectRest, IActorContext context);

        protected abstract Maybe<Unit> RegisterRespond(Maybe<IExposedReceiveActor> actor);

        protected Maybe<Unit> RegisterRespond<TData>(IEventSource<TData> source, Func<Maybe<TData>, IEnumerable<Issue.IssueCompleter>> validator)
        {
            return from actor in Actor
                   select Action(() => actor.RespondOnEventSource(source,
                                                                  data => SendIssues(validator(data),
                                                                      ExposedReceiveActor.ExposedContext)));
        }
    }
}