using System;
using Tauron.Application.Master.Commands.Administration.Host;

namespace ServiceHost.ApplicationRegistry
{
    public sealed class InstalledApp : IEquatable<InstalledApp>
    {
        public static readonly InstalledApp Empty = new InstalledApp(string.Empty, string.Empty, 0, AppType.Cluster, true, string.Empty);

        public string Name { get; }

        public string Path { get; }

        public int Version { get; }

        public AppType AppType { get; }

        public bool SuressWindow { get; }

        public string Exe { get; }

        public InstalledApp(string name, string path, int version, AppType appType, bool suressWindow, string exe)
        {
            Name = name;
            Path = path;
            Version = version;
            AppType = appType;
            SuressWindow = suressWindow;
            Exe = exe;
        }

        public InstalledApp NewVersion()
            => new InstalledApp(Name, Path, Version + 1, AppType, SuressWindow, Exe);

        public bool Equals(InstalledApp? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name && Version == other.Version;
        }

        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is InstalledApp other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Name, Version);

        public static bool operator ==(InstalledApp? left, InstalledApp? right) => Equals(left, right);

        public static bool operator !=(InstalledApp? left, InstalledApp? right) => !Equals(left, right);
    }
}