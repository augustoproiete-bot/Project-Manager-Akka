using System;
using JetBrains.Annotations;

namespace Tauron.Application.Master.Commands.Deployment.Build.Data
{
    [PublicAPI]
    public sealed class AppBinary 
    {
        public int FileVersion { get; }

        public string AppName { get; }

        public DateTime CreationTime { get; }

        public bool Deleted { get; }

        public string Commit { get; }

        public string Repository { get; }

        public AppBinary(string appName, int fileVersion, DateTime creationTime, bool deleted, string commit, string repository)
        {
            AppName = appName;
            FileVersion = fileVersion;
            CreationTime = creationTime;
            Deleted = deleted;
            Commit = commit;
            Repository = repository;
        }
    }
}