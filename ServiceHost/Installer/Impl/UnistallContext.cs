using ServiceHost.ApplicationRegistry;
using Tauron.Application.ActorWorkflow;

namespace ServiceHost.Installer.Impl
{
    public sealed class UnistallContext : IWorkflowContext
    {
        public Backup Backup { get; } = new Backup();

        public Recovery Recovery { get; } = new Recovery();

        public string Name { get; }

        public InstalledApp App { get; set; } = InstalledApp.Empty;

        public UnistallContext(string name)
        {
            Name = name;
        }
    }
}