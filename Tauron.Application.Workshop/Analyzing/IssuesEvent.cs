using System.Collections.Generic;

namespace Tauron.Application.Workshop.Analyzing
{
    public sealed record IssuesEvent(string RuleName, IEnumerable<Issue> Issues);
}