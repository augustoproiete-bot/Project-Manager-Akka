namespace Tauron.Application.Workshop.Analyzing
{
    public sealed class Issue
    {
        public Issue(string issueType, object? data)
        {
            IssueType = issueType;
            Data = data;
        }

        public string IssueType { get; }

        public object? Data { get; }
    }
}