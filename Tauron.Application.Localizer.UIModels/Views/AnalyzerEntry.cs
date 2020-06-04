namespace Tauron.Application.Localizer.UIModels.Views
{
    public sealed class AnalyzerEntry
    {
        public string RuleName { get; }

        public string ErrorName { get; }

        public string Project { get; }

        public string Message { get; }

        public AnalyzerEntry(string ruleName, string project, string message, string errorName)
        {
            RuleName = ruleName;
            Project = project;
            Message = message;
            ErrorName = errorName;
        }
    }
}