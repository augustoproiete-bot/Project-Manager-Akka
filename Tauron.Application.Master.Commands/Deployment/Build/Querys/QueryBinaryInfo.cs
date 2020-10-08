using System.IO;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace Tauron.Application.Master.Commands.Deployment.Build.Querys
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