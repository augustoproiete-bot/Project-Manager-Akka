using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed class LanguageChange : MutatingChange
    {
        public LanguageChange(ActiveLanguage language, string name)
        {
            Language = language;
            Name = name;
        }

        private ActiveLanguage Language { get; }

        private string Name { get; }

        public AddActiveLanguage ToEventData()
        {
            return new AddActiveLanguage(Language, Name);
        }
    }
}