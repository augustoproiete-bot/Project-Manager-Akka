using JetBrains.Annotations;

namespace Tauron.Application.Master.Commands.Administration.Host
{
    [PublicAPI]
    public sealed class HostApp
    {
        public string Name { get; }

        public string Path { get; }

        public int AppVersion { get; } 

        public AppType AppType { get; }

        public bool SupressWindow { get; }

        public string Exe { get; }

        public bool Running { get; }

        public HostApp(string name, string path, int version, AppType appType, bool supressWindow, string exe, bool running)
        {
            Name = name;
            Path = path;
            AppVersion = version;
            AppType = appType;
            SupressWindow = supressWindow;
            Exe = exe;
            Running = running;
        }
    }
}