using Akka.Actor;
using Akka.Actor.Dsl;
using Tauron.Application.Workshop.Analyzing.Rules;
using Tauron.Application.Workshop.Mutating;

namespace Tauron.Application.Localizer.DataModel.Workspace.Analyzing
{
    public abstract class ProjectRule : RuleBase<ProjectFileWorkspace, MutatingContext<ProjectFile>>
    {
        protected override void ActorConstruct(IActorDsl dsl, IActorContext context) 
            => dsl.Receive<ProjectRest>(ValidateAll);

        protected abstract void ValidateAll(ProjectRest projectRest, IActorContext context);

        protected override void RegisterResponds(ProjectFileWorkspace workspace, IActorContext context) 
            => workspace.Source.ProjectReset.RespondOn(context.Self);
    }
}