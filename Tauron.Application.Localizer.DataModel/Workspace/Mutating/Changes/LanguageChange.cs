using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed record LanguageChange(ActiveLanguage Language, string Name) : MutatingChange
    {
        public AddActiveLanguage ToEventData() => new(Language, Name);
    }
}