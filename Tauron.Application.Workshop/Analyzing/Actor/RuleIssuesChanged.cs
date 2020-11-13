using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Analyzing.Rules;

namespace Tauron.Application.Workshop.Analyzing.Actor
{
    [PublicAPI]
    public sealed record RuleIssuesChanged<TWorkspace, TData>(IRule<TWorkspace, TData> Rule, IEnumerable<Issue.IssueCompleter> Issues)
        where TWorkspace : WorkspaceBase<TData> where TData : class
    {
        public IssuesEvent ToEvent() 
            => new(Rule.Name, Issues.Select(i => i.Build(Rule.Name)));
    }
}