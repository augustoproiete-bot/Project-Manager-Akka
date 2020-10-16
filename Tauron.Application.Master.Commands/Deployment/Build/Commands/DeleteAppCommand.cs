using System.IO;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace Tauron.Application.Master.Commands.Deployment.Build.Commands
{
    public sealed class DeleteAppCommand : DeploymentCommandBase<AppInfo>
    {
        public DeleteAppCommand([NotNull] string appName) : base(appName)
        {
        }

        public DeleteAppCommand([NotNull] BinaryReader reader, [NotNull] ExtendedActorSystem system) : base(reader, system)
        {
        }
    }
}