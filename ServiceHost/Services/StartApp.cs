using ServiceHost.ApplicationRegistry;

namespace ServiceHost.Services
{
    public sealed class StartApp
    {
        public InstalledApp App { get; }

        public StartApp(InstalledApp app) 
            => App = app;
    }
}