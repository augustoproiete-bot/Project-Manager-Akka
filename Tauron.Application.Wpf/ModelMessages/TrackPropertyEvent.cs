namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed class TrackPropertyEvent
    {
        public string Name { get; }

        public TrackPropertyEvent(string name) => Name = name;
    }
}