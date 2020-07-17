using ServiceHost.ApplicationRegistry;

namespace ServiceHost.Installer
{
    public sealed class FileInstallationRequest
    {
        public string Name { get; }

        public string Path { get; }

        public bool Override { get; }
        public AppType AppType { get; }

        public FileInstallationRequest(string name, string path, bool @override, AppType appType)
        {
            Name = name;
            Path = path;
            Override = @override;
            AppType = appType;
        }
    }
}