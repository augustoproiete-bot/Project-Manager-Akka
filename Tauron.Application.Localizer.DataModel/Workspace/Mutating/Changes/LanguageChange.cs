namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed class LanguageChange : MutatingChange
    {
        private ActiveLanguage Language { get; }

        private string Name { get; }

        public LanguageChange(ActiveLanguage language, string name)
        {
            Language = language;
            Name = name;
        }

        public AddActiveLanguage ToEventData() => new AddActiveLanguage(Language, Name);
    }
}