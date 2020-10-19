using JetBrains.Annotations;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace Tauron.Application.Master.Commands.Deployment.Build.Querys
{
    [PublicAPI]
    public sealed class QueryBinaryInfo : DeploymentQueryBase<QueryBinaryInfo, BinaryList>
    {
        public QueryBinaryInfo([NotNull] string appName) 
            : base(appName)
        {
        }
    }
}