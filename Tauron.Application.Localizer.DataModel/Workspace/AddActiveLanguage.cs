namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class AddActiveLanguage
    {
        public AddActiveLanguage(ActiveLanguage activeLanguage, string projectName)
        {
            ActiveLanguage = activeLanguage;
            ProjectName = projectName;
        }

        public ActiveLanguage ActiveLanguage { get; }

        public string ProjectName { get; }
    }
}