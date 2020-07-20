using ServiceHost.ApplicationRegistry;

namespace ServiceHost.Services
{
    public sealed class StartApps
    {
        public AppType AppType { get; }

        public StartApps(AppType appType) 
            => AppType = appType;
    }
}