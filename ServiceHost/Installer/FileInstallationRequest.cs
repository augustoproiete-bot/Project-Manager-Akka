namespace ServiceHost.Installer
{
    public sealed class FileInstallationRequest
    {
        public string Name { get; }

        public string Path { get; }

        public bool Override { get; }

        public FileInstallationRequest(string name, string path, bool @override)
        {
            Name = name;
            Path = path;
            Override = @override;
        }
    }
}