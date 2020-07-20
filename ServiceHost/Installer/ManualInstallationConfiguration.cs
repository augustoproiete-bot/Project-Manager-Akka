using ServiceHost.ApplicationRegistry;

namespace ServiceHost.Installer
{
    public sealed class ManualInstallationConfiguration
    {
        public InstallType Install { get; set; } = InstallType.Empty;

        public string ZipFile { get; set; } = string.Empty;

        public string AppName { get; set; } = string.Empty;

        public bool Override { get; set; } = false;

        public string Exe { get; set; }

        public AppType AppType { get; set; } = AppType.StartUp;
    }
}