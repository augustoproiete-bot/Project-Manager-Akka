using System.Collections.Generic;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Analyzing.Rules;

namespace Tauron.Application.Workshop.Analyzing.Actor
{
    [PublicAPI]
    public sealed class RuleIssuesChanged<TWorkspace, TData>
        where TWorkspace : WorkspaceBase<TData>
    {
        public RuleIssuesChanged(IRule<TWorkspace, TData> rule, IEnumerable<Issue> issues)
        {
            Rule = rule;
            Issues = issues;
        }

        public IRule<TWorkspace, TData> Rule { get; }

        public IEnumerable<Issue> Issues { get; }

        public IssuesEvent ToEvent()
        {
            return new IssuesEvent(Rule.Name, Issues);
        }
    }
}