using System.IO;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace Tauron.Application.Master.Commands.Deployment.Build.Commands
{
    public sealed class PushVersionCommand : DeploymentCommandBase<AppBinary>
    {
        public PushVersionCommand([NotNull] string appName) : base(appName)
        {
        }

        public PushVersionCommand([NotNull] BinaryReader reader, [NotNull] ExtendedActorSystem system) : base(reader, system)
        {
        }
    }
}