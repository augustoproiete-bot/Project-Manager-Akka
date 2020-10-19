using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Tauron.Application.Master.Commands.Deployment.Build.Data
{
    public sealed class AppList : IEnumerable<AppInfo>
    {
        public ImmutableList<AppInfo> Apps { get; }

        public AppList(ImmutableList<AppInfo> apps) => Apps = apps;

        public IEnumerator<AppInfo> GetEnumerator() => Apps.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) Apps).GetEnumerator();
    }
}