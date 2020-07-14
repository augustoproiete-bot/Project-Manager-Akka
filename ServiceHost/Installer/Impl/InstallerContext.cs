using Tauron.Application.ActorWorkflow;

namespace ServiceHost.Installer.Impl
{
    public sealed class InstallerContext : IWorkflowContext
    {
        public InstallType Manual { get; }

        public string Name { get; }

        public string Path { get; }


        public bool Override { get; }

        public InstallerContext(InstallType manual, string name, string path, bool @override)
        {
            Manual = manual;
            Name = name;
            Path = path;
            Override = @override;
        }
    }
}