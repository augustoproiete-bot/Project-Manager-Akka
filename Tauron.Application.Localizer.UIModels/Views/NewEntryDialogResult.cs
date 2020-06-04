namespace Tauron.Application.Localizer.UIModels.Views
{
    public sealed class NewEntryDialogResult
    {
        public NewEntryDialogResult(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public abstract class NewEntryInfoBase
    {

    }

    public sealed class NewEntryInfo : NewEntryInfoBase
    {
        public string Name { get; }

        public NewEntryInfo(string name) => Name = name;
    }

    public sealed class NewEntrySuggestInfo : NewEntryInfoBase
    {
        public string Name { get; }

        public NewEntrySuggestInfo(string name) => Name = name;
    }
}