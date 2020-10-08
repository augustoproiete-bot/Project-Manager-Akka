using System.IO;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Master.Commands.Deployment.Deployment.Data;

namespace Tauron.Application.Master.Commands.Deployment.Deployment.Querys
{
    [PublicAPI]
    public sealed class QueryBinaryInfo : DeploymentQueryBase<BinaryList>
    {
        public QueryBinaryInfo([NotNull] string appName) 
            : base(appName)
        {
        }

        public QueryBinaryInfo([NotNull] BinaryReader reader, [NotNull] ExtendedActorSystem system) 
            : base(reader, system)
        {
        }
    }
}