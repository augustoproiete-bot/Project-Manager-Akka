using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Tauron.Application.Master.Commands.Deployment.Build.Data
{
    [PublicAPI]
    public sealed class AppInfo
    {
        public string Name { get; }

        public int LastVersion { get; }

        public DateTime UpdateDate { get; }

        public DateTime CreationTime { get; }

        public string Repository { get; }

        public ImmutableList<AppBinary> Binaries { get; }

        public bool Deleted { get; set; }

        public AppInfo(string name, int lastVersion, DateTime updateDate, DateTime creationTime, string repository, IEnumerable<AppBinary> bin)
        {
            Name = name;
            LastVersion = lastVersion;
            UpdateDate = updateDate;
            CreationTime = creationTime;
            Repository = repository;
            Binaries = ImmutableList<AppBinary>.Empty.AddRange(bin);
        }

        public AppInfo IsDeleted()
        {
            Deleted = true;
            return this;
        }
    }
}