using System.Collections.Generic;
using Tauron.Application.Localizer.DataModel.Workspace.Analyzing.Rules;

namespace Tauron.Application.Localizer.DataModel.Workspace.Analyzing.Actor
{
    public sealed class RuleIssuesChanged
    {
        public IRule Rule { get; }

        public IEnumerable<Issue> Issues { get; }

        public RuleIssuesChanged(IRule rule, IEnumerable<Issue> issues)
        {
            Rule = rule;
            Issues = issues;
        }
    }
}