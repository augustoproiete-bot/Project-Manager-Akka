using JetBrains.Annotations;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace Tauron.Application.Master.Commands.Deployment.Build.Commands
{
    public sealed class DeleteAppCommand : DeploymentCommandBase<DeleteAppCommand, AppInfo>
    {
        public DeleteAppCommand([NotNull] string appName) 
            : base(appName)
        {
        }
    }
}