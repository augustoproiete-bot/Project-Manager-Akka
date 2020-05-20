namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed class LanguageChange : MutatingChange
    {
        public ActiveLanguage Language { get; }

        public LanguageChange(ActiveLanguage language) => Language = language;
    }
}