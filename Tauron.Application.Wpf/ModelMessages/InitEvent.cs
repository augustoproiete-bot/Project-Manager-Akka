namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed class InitEvent
    {
        public string Key { get; }

        public InitEvent(string key) => Key = key;
    }
}