namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed class UnloadEvent
    {
        public UnloadEvent(string key)
        {
            Key = key;
        }

        public string Key { get; }
    }
}