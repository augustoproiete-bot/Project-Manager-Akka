using System;
using System.IO;
using JetBrains.Annotations;

namespace ServiceManager.ProjectRepository
{
    [PublicAPI]
    public class RepositoryConfiguration
    {
        public const string RepositoryLockName = "{01324768-0950-49F9-AB39-CA94E4252C55}-ServiceManager-ProjectManagerRepository";

        private const string DefaultDotNetPath = @"C:\Program Files (x86)\dotnet\dotnet.exe";
        private const string DefaultSoloution = "Project-Manager-Akka.sln";
        private const string DefaultUrl = "https://github.com/Tauron1990/Project-Manager-Akka.git";

        public string DotNetPath { get; set; } = DefaultDotNetPath;

        public string Solotion { get; set; } = DefaultSoloution;

        public string CloneUrl { get; set; } = DefaultUrl;

        public string SourcePath { get; set; } = Path.Combine(Env.Path, "Git");

        public string TrackingId { get; set; } = "Unkowen";

        public Action<string, object[]>? Logger { get; }

        public RepositoryConfiguration(Action<string, object[]>? logger = null)
        {
            Logger = logger;
        }
    }
}
