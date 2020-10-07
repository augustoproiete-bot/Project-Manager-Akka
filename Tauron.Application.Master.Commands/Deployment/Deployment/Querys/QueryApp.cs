using System.IO;
using Akka.Actor;
using Tauron.Application.Master.Commands.Deployment.Deployment.Data;

namespace Tauron.Application.Master.Commands.Deployment.Deployment.Querys
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