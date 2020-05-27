namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class EntryRemove
    {
        public LocEntry Entry { get; }

        public EntryRemove(LocEntry entry) => Entry = entry;
    }
}