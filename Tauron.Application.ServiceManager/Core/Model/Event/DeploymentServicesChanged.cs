namespace Tauron.Application.ServiceManager.Core.Model.Event
{
    public sealed class DeploymentServicesChanged
    {
        public bool Ready { get; }

        public DeploymentServicesChanged(bool? ready) => Ready = ready == true;
    }
}