namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class EntryRemove
    {
        public EntryRemove(LocEntry entry)
        {
            Entry = entry;
        }

        public LocEntry Entry { get; }
    }
}