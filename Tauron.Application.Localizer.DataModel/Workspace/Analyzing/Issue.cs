namespace Tauron.Application.Localizer.DataModel.Workspace.Analyzing
{
    public sealed class Issue
    {
        public IssueType IssueType { get; }

        public object? Data { get; }

        public Issue(IssueType issueType, object? data)
        {
            IssueType = issueType;
            Data = data;
        }
    }
}