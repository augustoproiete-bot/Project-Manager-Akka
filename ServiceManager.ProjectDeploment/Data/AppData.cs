using System;
using System.Collections.Generic;

namespace ServiceManager.ProjectDeployment.Data
{
    public sealed class AppData
    {
        public List<AppFileInfo> Versions { get; set; } = new List<AppFileInfo>();

        public string Name { get; set; }

        public AppVersion Last { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime LastUpdate { get; set; }
    }
}