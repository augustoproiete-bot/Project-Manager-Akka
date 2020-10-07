using System.Collections.Generic;

namespace ServiceManager.ProjectDeployment.Data
{
    public sealed class AppData
    {
        public List<AppVersion> Versions { get; set; } = new List<AppVersion>();

        public string Name { get; set; }

        public AppVersion Last { get; set; }
    }
}