namespace Tauron.Application.Localizer.UIModels
{
    public sealed class ProjectViewLanguageModel
    {
        public string Name { get; }

        public bool IsEnabled { get; }

        public ProjectViewLanguageModel(string name, bool isEnabled)
        {
            Name = name;
            IsEnabled = isEnabled;
        }
    }
}