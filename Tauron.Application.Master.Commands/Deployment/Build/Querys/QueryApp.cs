using System.IO;
using Akka.Actor;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace Tauron.Application.Master.Commands.Deployment.Build.Querys
{
    public sealed class QueryApp : DeploymentQueryBase<AppInfo>
    {
        public QueryApp(string appName) 
            : base(appName)
        {
        }

        public QueryApp(BinaryReader reader, ExtendedActorSystem system) : base(reader, system)
        {
        }
    }
}