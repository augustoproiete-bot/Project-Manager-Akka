using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace Tauron.Application.Master.Commands.Deployment.Build.Querys
{
    public sealed class QueryChangeSource : DeploymentQueryBase<QueryChangeSource, AppChangedSource>
    {
        public QueryChangeSource() : base("All")
        {
        }
    }
}