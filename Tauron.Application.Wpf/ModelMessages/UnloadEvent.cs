namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed class UnloadEvent
    {
        public string Key { get; }

        public UnloadEvent(string key) => Key = key;
    }
}