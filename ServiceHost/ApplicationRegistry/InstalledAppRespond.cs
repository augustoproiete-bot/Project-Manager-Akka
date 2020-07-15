namespace ServiceHost.ApplicationRegistry
{
    public class InstalledAppRespond
    {
        public InstalledApp App { get; }

        public InstalledAppRespond(InstalledApp app) => App = app;
    }
}