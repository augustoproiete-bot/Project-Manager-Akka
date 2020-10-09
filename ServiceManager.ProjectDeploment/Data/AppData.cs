using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ServiceManager.ProjectDeployment.Data
{
    public sealed class AppData
    {
        public List<AppFileInfo> Versions { get; set; } = new List<AppFileInfo>();

        [BsonId]
        public string Name { get; set; }

        public AppVersion Last { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime LastUpdate { get; set; }

        public string Repository { get; set; }

        public string ProjectName { get; set; }
    }
}