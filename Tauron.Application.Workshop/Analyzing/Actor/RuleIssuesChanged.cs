using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Analyzing.Rules;

namespace Tauron.Application.Workshop.Analyzing.Actor
{
    [PublicAPI]
    public sealed class RuleIssuesChanged<TWorkspace, TData>
        where TWorkspace : WorkspaceBase<TData> where TData : class
    {
        public RuleIssuesChanged(IRule<TWorkspace, TData> rule, IEnumerable<Issue.IssueCompleter> issues)
        {
            Rule = rule;
            Issues = issues;
        }

        public IRule<TWorkspace, TData> Rule { get; }

        public IEnumerable<Issue.IssueCompleter> Issues { get; }

        public IssuesEvent ToEvent() 
            => new IssuesEvent(Rule.Name, Issues.Select(i => i.Build(Rule.Name)));
    }
}