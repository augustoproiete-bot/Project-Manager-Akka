using System.Collections.Generic;

namespace Tauron.Application.Workshop.Analyzing
{
    public sealed class IssuesEvent
    {
        public string RuleName { get; }

        public IEnumerable<Issue> Issues { get; }

        public IssuesEvent(string ruleName, IEnumerable<Issue> issues)
        {
            RuleName = ruleName;
            Issues = issues;
        }
    }
}