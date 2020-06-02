using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed class GlobalLanguageChange : MutatingChange
    {
        public GlobalLanguageChange(ActiveLanguage language)
        {
            Language = language;
        }

        public ActiveLanguage Language { get; }
    }
}