using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed class EntryChange : MutatingChange
    {
        public LocEntry Entry { get; }

        public EntryChange(LocEntry entry) => Entry = entry;
    }
}