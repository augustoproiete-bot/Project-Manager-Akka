using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace Tauron.Application.Master.Commands.Deployment.Build.Commands
{
    public sealed class CreateAppCommand : DeploymentCommandBase<CreateAppCommand, AppInfo>
    {
        public string TargetRepo { get; }

        public string ProjectName { get; } 

        public CreateAppCommand(string appName, string targetRepo, string projectName)
            : base(appName)
        {
            TargetRepo = targetRepo;
            ProjectName = projectName;
        }
    }
}