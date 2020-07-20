namespace ServiceHost.Installer
{
    public sealed class UninstallApp
    {
        public string Name { get; }

        public UninstallApp(string name) => Name = name;
    }
}