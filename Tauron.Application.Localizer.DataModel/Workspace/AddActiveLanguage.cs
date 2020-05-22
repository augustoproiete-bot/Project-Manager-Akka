namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class AddActiveLanguage
    {
        public ActiveLanguage ActiveLanguage { get; }

        public string ProjectName { get; }

        public AddActiveLanguage(ActiveLanguage activeLanguage, string projectName)
        {
            ActiveLanguage = activeLanguage;
            ProjectName = projectName;
        }
    }
}