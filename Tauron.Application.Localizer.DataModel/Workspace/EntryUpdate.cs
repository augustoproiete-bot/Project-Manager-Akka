namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class EntryUpdate
    {
        public LocEntry Entry { get; }


        public EntryUpdate(LocEntry entry) => Entry = entry;
    }
}