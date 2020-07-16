namespace ServiceHost.ApplicationRegistry
{
    public class InstalledAppRespond
    {
        public InstalledApp App { get; }

        public bool Fault { get; set; }

        public InstalledAppRespond(InstalledApp app) => App = app;
    }
}