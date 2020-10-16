using System.IO;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace Tauron.Application.Master.Commands.Deployment.Build.Querys
{
    public sealed class QueryChangeSource : DeploymentQueryBase<AppChangedSource>
    {
        public QueryChangeSource() : base("All")
        {
        }

        public QueryChangeSource([NotNull] BinaryReader reader, [NotNull] ExtendedActorSystem system) : base(reader, system)
        {
        }
    }
}