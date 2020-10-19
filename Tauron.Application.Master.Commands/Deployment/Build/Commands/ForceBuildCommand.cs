using Tauron.Application.AkkNode.Services.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Build.Commands
{
    public sealed class ForceBuildCommand : FileTransferCommand<DeploymentApi, ForceBuildCommand>, IDeploymentCommand
    {
        public string Repository { get; }
        public string Project { get; }

        public ForceBuildCommand(string repository, string project)
        {
            Repository = repository;
            Project = project;
        }

        protected override string Info => $"{Repository}.{Project}";
        public string AppName => "No";
    }
}