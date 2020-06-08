namespace Tauron.Application.Localizer.UIModels.Views
{
    public sealed class AnalyzerEntry
    {
        public sealed class Builder
        {
            private readonly string _ruleName;
            private readonly string _project;

            public Builder(string ruleName, string project)
            {
                _ruleName = ruleName;
                _project = project;
            }

            public AnalyzerEntry Entry(string errorName, string message)
                => new AnalyzerEntry(_ruleName, _project, message, errorName);
        }

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