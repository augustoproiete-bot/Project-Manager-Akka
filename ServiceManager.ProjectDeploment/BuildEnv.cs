using System;
using System.IO;
using Tauron.Temp;

namespace ServiceManager.ProjectDeployment
{
    public static class BuildEnv
    {
        private static readonly string ApplicationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tauron", "DeploymentServer");

        private static TempStorage? _storage;

        public static TempStorage TempFiles => _storage ??= TempStorage.CleanAndCreate(ApplicationPath);
    }

}