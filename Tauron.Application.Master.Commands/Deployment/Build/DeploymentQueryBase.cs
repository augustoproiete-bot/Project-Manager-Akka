using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Build
{
    [PublicAPI]
    public abstract class DeploymentQueryBase<TThis, TResult> : ResultCommand<DeploymentApi, TThis, TResult>, IDeploymentQuery 
        where TThis : ResultCommand<DeploymentApi, TThis, TResult>
    {
        public string AppName { get; }

        protected override string Info => AppName;

        protected DeploymentQueryBase(string appName) => AppName = appName;
    }
}