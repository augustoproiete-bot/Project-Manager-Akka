using System;
using System.Collections.Immutable;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace ServiceManager.ProjectDeployment.Data
{
    public sealed class AppData
    {
        public static readonly AppData Empty = new AppData("", -1, DateTime.MinValue, DateTime.MinValue, "", "", ImmutableList<AppFileInfo>.Empty);

        public ImmutableList<AppFileInfo> Versions { get; }

        [BsonId, BsonElement]
        public string Name { get; }

        [BsonElement]
        public int Last { get; }

        [BsonElement]
        public DateTime CreationTime { get; }

        [BsonElement]
        public DateTime LastUpdate { get; }

        [BsonElement]
        public string Repository { get; }

        [BsonElement]
        public string ProjectName { get; }

        [BsonConstructor]
        public AppData(string name, int last, DateTime creationTime, DateTime lastUpdate, string repository, string projectName, ImmutableList<AppFileInfo> versions)
        {
            Name = name;
            Last = last;
            CreationTime = creationTime;
            LastUpdate = lastUpdate;
            Repository = repository;
            ProjectName = projectName;
            Versions = versions;
        }

        public AppInfo ToInfo()
            => new AppInfo(Name, Last, LastUpdate, CreationTime, Repository, Versions.Select(fi => new AppBinary(Name, fi.Version, fi.CreationTime, fi.Deleted, fi.Commit, Repository)));
    }
}