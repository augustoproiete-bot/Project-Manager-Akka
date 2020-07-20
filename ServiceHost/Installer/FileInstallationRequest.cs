using ServiceHost.ApplicationRegistry;
using ServiceHost.Installer.Impl;

namespace ServiceHost.Installer
{
    public sealed class FileInstallationRequest : InstallRequest
    {
        public string Name { get; }

        public string Path { get; }

        public bool Override { get; }

        public AppType AppType { get; }

        public string Exe { get; }

        public FileInstallationRequest(string name, string path, bool @override, AppType appType, string exe)
        {
            Name = name ?? string.Empty;
            Path = path ?? string.Empty;
            Override = @override;
            AppType = appType;
            Exe = exe ?? string.Empty;
        }
    }
}