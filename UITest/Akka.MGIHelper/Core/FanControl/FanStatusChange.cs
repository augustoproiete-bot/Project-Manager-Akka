namespace Akka.MGIHelper.Core.FanControl
{
    public sealed class FanStatusChange
    {
        public bool Running { get; }

        public FanStatusChange(bool running) => Running = running;
    }
}