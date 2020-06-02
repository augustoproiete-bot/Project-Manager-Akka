namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating
{
    public sealed class EntryAdd
    {
        public EntryAdd(LocEntry entry)
        {
            Entry = entry;
        }

        public LocEntry Entry { get; }
    }
}