using JetBrains.Annotations;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace Tauron.Application.Master.Commands.Deployment.Build.Commands
{
    public sealed class PushVersionCommand : DeploymentCommandBase<PushVersionCommand, AppBinary>
    {
        public PushVersionCommand([NotNull] string appName) : base(appName)
        {
        }
    }
}