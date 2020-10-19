namespace Tauron.Application.Master.Commands.Deployment.Build
{
    public interface IDeploymentCommand
    {
        string AppName { get; }
    }
}