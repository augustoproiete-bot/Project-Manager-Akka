using System.IO;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Master.Commands.Deployment.Deployment.Data;

namespace Tauron.Application.Master.Commands.Deployment.Deployment.Querys
{
    [PublicAPI]
    public sealed class QueryApps : DeploymentQueryBase<AppList>
    {
        public QueryApps() 
            : base("All")
        {
        }

        public QueryApps(BinaryReader reader, ExtendedActorSystem system) 
            : base(reader, system)
        {
        }
    }
}