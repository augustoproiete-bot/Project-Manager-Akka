namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class EntryUpdate
    {
        public EntryUpdate(LocEntry entry)
        {
            Entry = entry;
        }

        public LocEntry Entry { get; }
    }
}