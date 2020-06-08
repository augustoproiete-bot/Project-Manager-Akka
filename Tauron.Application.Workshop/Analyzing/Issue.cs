namespace Tauron.Application.Workshop.Analyzing
{
    public sealed class Issue
    {
        public sealed class IssueCompleter
        {
            private readonly string _type;
            private readonly string _project;
            private readonly object _data;

            public IssueCompleter(string type, string project, object data)
            {
                _type = type;
                _project = project;
                _data = data;
            }

            public Issue Build(string ruleName)
                => new Issue(_type, _data, _project, ruleName);
        }

        public Issue(string issueType, object? data, string project, string ruleName)
        {
            IssueType = issueType;
            Data = data;
            Project = project;
            RuleName = ruleName;
        }

        public string RuleName { get; }

        public string IssueType { get; }

        public string Project { get; }

        public object? Data { get; }

        public static IssueCompleter New(string type)
            => new IssueCompleter(type, string.Empty, null);
    }
}