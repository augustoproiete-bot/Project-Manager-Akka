using JetBrains.Annotations;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace Tauron.Application.Master.Commands.Deployment.Build.Querys
{
    [PublicAPI]
    public sealed class QueryApps : DeploymentQueryBase<QueryApps, AppList>
    {
        public QueryApps() 
            : base("All")
        {
        }
    }
}