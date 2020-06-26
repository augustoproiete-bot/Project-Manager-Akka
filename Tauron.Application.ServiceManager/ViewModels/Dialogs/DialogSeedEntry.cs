namespace Tauron.Application.ServiceManager.ViewModels.Dialogs
{
    public sealed class DialogSeedEntry
    {
        public string Url { get; }

        public DialogSeedEntry(string url) => Url = url;
    }
}