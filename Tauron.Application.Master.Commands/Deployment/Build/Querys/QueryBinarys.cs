using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Build.Querys
{
    [PublicAPI]
    public sealed class QueryBinarys : FileTransferCommand<DeploymentApi, QueryBinarys>
    {
        public string AppName { get; }
        public int AppVersion { get; private set; }

        public QueryBinarys(string appName, int appVersion = -1)
        {
            AppName = appName;
            AppVersion = appVersion;
        }

        protected override string Info => AppName;
    }
}