namespace Tauron.Application.Workshop.Analyzing
{
    public sealed class Issue
    {
        public string IssueType { get; }

        public object? Data { get; }

        public Issue(string issueType, object? data)
        {
            IssueType = issueType;
            Data = data;
        }
    }
}