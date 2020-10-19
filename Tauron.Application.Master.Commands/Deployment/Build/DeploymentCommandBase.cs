using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Build
{
    [PublicAPI]
    public abstract class DeploymentCommandBase<TThis, TResult> : ResultCommand<DeploymentApi, TThis, TResult>, IDeploymentCommand 
        where TThis : ResultCommand<DeploymentApi, TThis, TResult>
    {
        public string AppName { get; }

        protected override string Info => AppName;

        protected DeploymentCommandBase(string appName) 
            => AppName = appName;

    }
}