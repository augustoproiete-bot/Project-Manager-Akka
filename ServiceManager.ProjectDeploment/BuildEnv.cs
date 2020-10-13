using System;
using System.IO;

namespace ServiceManager.ProjectDeployment
{
    public static class BuildEnv
    {
        public static readonly string ApplicationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tauron", "DeploymentServer");
    }

}