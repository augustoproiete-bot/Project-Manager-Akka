using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed class RemoveEntryChange : MutatingChange
    {
        public RemoveEntryChange(LocEntry entry)
        {
            Entry = entry;
        }

        public LocEntry Entry { get; }
    }
}