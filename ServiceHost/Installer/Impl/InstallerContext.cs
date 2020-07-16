using System;
using ServiceHost.ApplicationRegistry;
using ServiceHost.Installer.Impl.Source;
using Tauron.Application.ActorWorkflow;

namespace ServiceHost.Installer.Impl
{
    public sealed class InstallerContext : IWorkflowContext
    {
        public Recovery Recovery { get; } = new Recovery();

        public Backup Backup { get; } = new Backup();

        public InstallType Manual { get; }

        public string Name { get; }

        public object SourceLocation { get; }

        public IInstallationSource Source { get; private set; } = EmptySource.Instnace;

        public InstalledApp InstalledApp { get; private set; } = InstalledApp.Empty;

        public bool Override { get; }

        public string InstallationPath { get; set; } = string.Empty;

        public InstallerContext(InstallType manual, string name, string sourceLocation, bool @override)
        {
            Manual = manual;
            Name = name;
            SourceLocation = sourceLocation;
            Override = @override;
        }

        public IInstallationSource? SetSource(Func<InstallerContext, IInstallationSource> source, Action<string> setError)
        {
            Source = source(this);
            if (Source == EmptySource.Instnace)
                setError(ErrorCodes.NoSourceFound);
            return Source;
        }

        public void SetInstalledApp(InstalledApp app)
            => InstalledApp = app;
    }
}