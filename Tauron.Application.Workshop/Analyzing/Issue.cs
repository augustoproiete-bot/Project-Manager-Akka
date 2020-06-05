namespace Tauron.Application.Workshop.Analyzing
{
    public sealed class Issue
    {
        public Issue(string issueType, object? data, string project)
        {
            IssueType = issueType;
            Data = data;
            Project = project;
        }

        public string IssueType { get; }

        public string Project { get; }

        public object? Data { get; }
    }
}