using System.Collections.Generic;
using Akka.Actor;
using Akka.Actor.Dsl;
using Tauron.Application.Workshop.Analyzing;

namespace Tauron.Application.Localizer.DataModel.Workspace.Analyzing
{
    public sealed class SourceRule : ProjectRule
    {
        public const string SourceRuleName = "SourceCheck";

        public override string Name => SourceRuleName;
        
        protected override void ValidateAll(ProjectRest projectRest, IActorContext context) => ValidateSource(projectRest.ProjectFile.Source, context);

        protected override void RegisterResponds(ProjectFileWorkspace workspace, IActorContext context) => workspace.Source.SourceUpdate.RespondOn(context.Self);

        protected override void RegisterRules(IActorDsl dsl) => dsl.Receive<SourceUpdated>(((updated, context) => ValidateSource(updated.Source, context)));

        private void ValidateSource(string source, IActorContext context)
        {
            var issues = new List<Issue>();

            if(string.IsNullOrWhiteSpace(source))
                issues.Add(new Issue(Issues.EmptySource, null));

            SendIssues(issues, context);
        }
    }
}