namespace MGIHelper.Core.FanControl.Events
{
    public class TrackingEvent
    {
        public bool Error { get; }

        public string Reason { get; }

        public int Power { get; }

        public State State { get; }

        public double Pidout { get; }

        public int PidSetValue { get; }

        public int Pt1000 { get; }

        public TrackingEvent(int power, State state, double pidout, int pidSetValue, int pt1000)
        {
            Power = power;
            State = state;
            Pidout = pidout;
            PidSetValue = pidSetValue;
            Pt1000 = pt1000;
        }

        public TrackingEvent(bool error, string reason)
        {
            Error = error;
            Reason = reason;
        }
    }
}