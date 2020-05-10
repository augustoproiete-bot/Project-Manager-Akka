namespace Akka.MGIHelper.Core.FanControl.Events
{
    public class ClockEvent
    {
        public ClockEvent(ClockState clockState)
        {
            ClockState = clockState;
        }

        public ClockState ClockState { get; }
    }
}