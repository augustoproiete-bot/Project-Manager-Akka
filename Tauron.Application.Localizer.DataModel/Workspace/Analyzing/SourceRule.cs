using System.Collections.Generic;
using Akka.Actor;
using Functional.Maybe;
using Tauron.Akka;
using Tauron.Application.Workshop.Analyzing;
using static Tauron.Prelude;

namespace Tauron.Application.Localizer.DataModel.Workspace.Analyzing
{
    public sealed class SourceRule : LocalizerRule
    {
        public const string SourceRuleName = "SourceCheck";

        public override string Name => SourceRuleName;

        protected override IEnumerable<Issue.IssueCompleter> ValidateAll(ProjectRest projectRest, IActorContext context) 
            => ValidateSource(May(new SourceUpdated(projectRest.ProjectFile.Source)));

        protected override Maybe<Unit> RegisterRespond(Maybe<IExposedReceiveActor> actor) 
            => from workspace in Workspace
               select RegisterRespond(workspace.Source.SourceUpdate, ValidateSource);

        private static IEnumerable<Issue.IssueCompleter> ValidateSource(Maybe<SourceUpdated> arg)
        {
            if(arg.IsNothing())
                yield break;

            var source = arg.Value.Source;

            if (string.IsNullOrWhiteSpace(source))
                yield return Issue.New(Issues.EmptySource);
        }
    }
}