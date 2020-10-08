using System;
using MongoDB.Bson;

namespace ServiceManager.ProjectDeployment.Data
{
    public sealed class AppFileInfo
    {
        public ObjectId File { get; set; }

        public AppVersion Version { get; set; } = new AppVersion { Version = -1 };

        public DateTime CreationTime { get; set; }

        public bool Deleted { get; set; }

        public string Commit { get; set; }
    }
}