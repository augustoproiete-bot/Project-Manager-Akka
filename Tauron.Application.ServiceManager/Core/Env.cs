using System;

namespace Tauron.Application.ServiceManager.Core
{
    public static class Env
    {
        public static string Path { get; } = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tauron\\ServiceManager");
    }
}