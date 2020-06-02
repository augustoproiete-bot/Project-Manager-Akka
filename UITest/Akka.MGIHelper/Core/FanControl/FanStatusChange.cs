namespace Akka.MGIHelper.Core.FanControl
{
    public sealed class FanStatusChange
    {
        public FanStatusChange(bool running)
        {
            Running = running;
        }

        public bool Running { get; }
    }
}