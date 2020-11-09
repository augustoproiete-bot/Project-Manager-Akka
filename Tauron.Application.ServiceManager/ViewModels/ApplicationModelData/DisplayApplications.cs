namespace Tauron.Application.ServiceManager.ViewModels.ApplicationModelData
{
    public sealed class DisplayApplications
    {
        public string HostName { get; }

        public DisplayApplications(string hostName) 
            => HostName = hostName;
    }
}