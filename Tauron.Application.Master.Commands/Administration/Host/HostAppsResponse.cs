using System.Collections.Immutable;

namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed class HostAppsResponse
    {
        public ImmutableList<HostApp> Apps { get; private set; } = ImmutableList<HostApp>.Empty;

        public HostAppsResponse(ImmutableList<HostApp> apps) => Apps = apps;
    }
}