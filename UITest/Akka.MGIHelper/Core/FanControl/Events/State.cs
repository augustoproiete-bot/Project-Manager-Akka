namespace Akka.MGIHelper.Core.FanControl.Events
{
    public enum State
    {
        Error = 0,
        Idle,
        Ready,
        Ignition,
        StartUp,
        StandBy,
        Power,
        Cooldown,
        TestRun
    }
}