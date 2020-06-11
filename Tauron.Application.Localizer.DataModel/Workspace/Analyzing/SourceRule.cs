using System.Collections.Generic;
using Akka.Actor;
using Tauron.Akka;
using Tauron.Application.Workshop.Analyzing;

namespace Tauron.Application.Localizer.DataModel.Workspace.Analyzing
{
    public sealed class SourceRule : LocalizerRule
    {
        public const string SourceRuleName = "SourceCheck";

        public override string Name => SourceRuleName;

        protected override IEnumerable<Issue.IssueCompleter> ValidateAll(ProjectRest projectRest, IActorContext context) 
            => ValidateSource(new SourceUpdated(projectRest.ProjectFile.Source));

        protected override void RegisterRespond(IExposedReceiveActor actor)
        {
            RegisterRespond(Workspace.Source.SourceUpdate, ValidateSource);
        }

        private static IEnumerable<Issue.IssueCompleter> ValidateSource(SourceUpdated arg)
        {
            if (string.IsNullOrWhiteSpace(arg.Source))
                yield return Issue.New(Issues.EmptySource);
        }
    }
}