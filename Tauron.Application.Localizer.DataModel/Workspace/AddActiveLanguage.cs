namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class AddActiveLanguage
    {
        public ActiveLanguage ActiveLanguage { get; }

        public AddActiveLanguage(ActiveLanguage activeLanguage) => ActiveLanguage = activeLanguage;
    }
}