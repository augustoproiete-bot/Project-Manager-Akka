using System;
using ServiceHost.ApplicationRegistry;
using ServiceHost.Installer.Impl.Source;
using Tauron.Application.ActorWorkflow;

namespace ServiceHost.Installer.Impl
{
    public sealed class InstallerContext : IWorkflowContext
    {
        public InstallType Manual { get; }

        public string Name { get; }

        public string Path { get; }

        public IInstallationSource? Source { get; private set; }

        public InstalledApp InstalledApp { get; private set; }

        public bool Override { get; }

        public InstallerContext(InstallType manual, string name, string path, bool @override)
        {
            Manual = manual;
            Name = name;
            Path = path;
            Override = @override;
        }

        public IInstallationSource? SetSource(Func<InstallerContext, IInstallationSource?> source, Action<string> setError)
        {
            Source = source(this);
            if (Source == null)
                setError(ErrorCodes.NoSourceFound);
            return Source;
        }
    }
}