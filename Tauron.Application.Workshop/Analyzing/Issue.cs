using Functional.Maybe;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.Analyzing
{
    [PublicAPI]
    public sealed record Issue(string RuleName, string IssueType, string Project, Maybe<object> Data)
    {
        public sealed class IssueCompleter
        {
            private readonly string _type;
            private readonly string _project;
            private readonly Maybe<object> _data;

            public IssueCompleter(string type, string project, Maybe<object> data)
            {
                _type = type;
                _project = project;
                _data = data;
            }

            public Issue Build(string ruleName)
                => new (ruleName, _type, _project, _data);
        }


        public static IssueCompleter New(string type)
            => new IssueCompleter(type, string.Empty, Maybe<object>.Nothing);
    }
}