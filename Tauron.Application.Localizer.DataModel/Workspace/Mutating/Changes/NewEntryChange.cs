using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed class NewEntryChange : MutatingChange
    {
        private readonly LocEntry _newEntry;

        public NewEntryChange(LocEntry newEntry)
        {
            _newEntry = newEntry;
        }

        public EntryAdd ToData()
        {
            return new EntryAdd(_newEntry);
        }
    }
}