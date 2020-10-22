using System;
using JetBrains.Annotations;
using ServiceManager.ProjectRepository.Data;
using Tauron.Application.AkkNode.Services;

namespace ServiceManager.ProjectRepository.Core
{
    [PublicAPI]
    public class RepositoryConfiguration : IReporterProvider, IEquatable<RepositoryConfiguration>
    {
        //public const string RepositoryLockName = "{01324768-0950-49F9-AB39-CA94E4252C55}-ServiceManager-ProjectManagerRepository";

        //private const string DefaultDotNetPath = @"C:\Program Files\dotnet\dotnet.exe";
        //private const string DefaultSoloution = "Project-Manager-Akka.sln";
        private const string DefaultUrl = "https://github.com/Tauron1990/Project-Manager-Akka.git";

        //public string DotNetPath { get; set; } = DefaultDotNetPath;

        //public string Solotion { get; set; } = DefaultSoloution;

        public string CloneUrl { get; } = DefaultUrl;
        
        public Reporter? Logger { get; }

        public RepositoryConfiguration(Reporter? logger, RepositoryEntry entry)
        {
            Logger = logger;
            CloneUrl = entry.SourceUrl;
        }

        public RepositoryConfiguration()
        {
            
        }

        void IReporterProvider.SendMessage(string msg) => Logger?.Send(msg);

        public bool Equals(RepositoryConfiguration? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return CloneUrl == other.CloneUrl && Equals(Logger, other.Logger);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RepositoryConfiguration) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CloneUrl, Logger);
        }

        public static bool operator ==(RepositoryConfiguration? left, RepositoryConfiguration? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RepositoryConfiguration? left, RepositoryConfiguration? right)
        {
            return !Equals(left, right);
        }
    }
}
