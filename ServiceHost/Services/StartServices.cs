using ServiceHost.ApplicationRegistry;

namespace ServiceHost.Services
{
    public sealed class StartServices
    {
        public AppType AppType { get; }

        public StartServices(AppType appType) 
            => AppType = appType;
    }
}