using ServiceHost.ApplicationRegistry;
using Tauron.Application.Master.Commands.Administration.Host;

namespace ServiceHost.Services
{
    public sealed class StartApps
    {
        public AppType AppType { get; }

        public StartApps(AppType appType) 
            => AppType = appType;
    }
}