using Akka;
using Akka.Streams.Dsl;

namespace Tauron.Application.Master.Commands.Deployment.Build.Data
{
    public sealed class AppChangedSource
    {
        public Source<AppInfo, NotUsed> AppSource { get; }

        public AppChangedSource(Source<AppInfo, NotUsed> appSource) => AppSource = appSource;
    }
}