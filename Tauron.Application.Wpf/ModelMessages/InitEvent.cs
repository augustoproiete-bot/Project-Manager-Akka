namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed class InitEvent
    {
        public InitEvent(string key)
        {
            Key = key;
        }

        public string Key { get; }
    }
}