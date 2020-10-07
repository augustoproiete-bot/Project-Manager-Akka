using ServiceHost.ApplicationRegistry;
using Tauron.Application.Master.Commands.Administration.Host;

namespace ServiceHost.Installer
{
    public sealed class InstallerationCompled
    {
        public bool Succesfull { get; }

        public string Error { get; }

        public AppType Type { get; }

        public string Name { get; }

        public InstallationAction InstallAction { get; }

        public InstallerationCompled(bool succesfull, string error, AppType type, string name, InstallationAction installAction)
        {
            Succesfull = succesfull;
            Error = error;
            Type = type;
            Name = name;
            InstallAction = installAction;
        }
    }
}