using System.Collections.Generic;
using Akka.Actor;
using Akka.Actor.Dsl;
using Tauron.Application.Localizer.DataModel.Workspace.Analyzing.Actor;

namespace Tauron.Application.Localizer.DataModel.Workspace.Analyzing.Rules
{
    public abstract  class RuleBase : IRule
    {
        public abstract string Name { get; }

        public IActorRef Init(IActorRefFactory superviser, ProjectFileWorkspace workspace)
        {
            return superviser.ActorOf((dsl, contxt) =>
            {
                workspace.Source.ProjectReset.RespondOn(contxt.Self);
                dsl.Receive<ProjectRest>(ValidateAll);

                RegisterResponds(workspace, contxt);
                RegisterRules(dsl);
            }, Name);
        }

        protected abstract void ValidateAll(ProjectRest projectRest, IActorContext context);

        protected abstract void RegisterResponds(ProjectFileWorkspace workspace, IActorContext context);

        protected abstract void RegisterRules(IActorDsl dsl);

        protected void SendIssues(IEnumerable<Issue> issues, IActorContext context)
            => context.Parent.Tell(new RuleIssuesChanged(this, issues));
    }
}