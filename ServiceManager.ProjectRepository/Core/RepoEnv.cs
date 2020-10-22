using System;
using Tauron;
using Tauron.Temp;

namespace ServiceManager.ProjectRepository.Core
{
    public static class RepoEnv
    {
        public static string DataPath { get; } = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tauron\\ReporitoryManager");

        private static TempStorage? _storage;

        public static TempStorage TempFiles => _storage ??= TempStorage.CleanAndCreate(DataPath.CombinePath("Temp"));
    }
}