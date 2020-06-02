namespace Tauron.Application.Localizer.UIModels
{
    public sealed class ProjectViewLanguageModel
    {
        public ProjectViewLanguageModel(string name, bool isEnabled)
        {
            Name = name;
            IsEnabled = isEnabled;
        }

        public string Name { get; }

        public bool IsEnabled { get; }
    }
}