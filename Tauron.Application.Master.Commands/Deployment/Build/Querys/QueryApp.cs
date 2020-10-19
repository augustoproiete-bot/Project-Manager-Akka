using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace Tauron.Application.Master.Commands.Deployment.Build.Querys
{
    public sealed class QueryApp : DeploymentQueryBase<QueryApp, AppInfo>
    {
        public QueryApp(string appName) 
            : base(appName)
        { }

    }
}