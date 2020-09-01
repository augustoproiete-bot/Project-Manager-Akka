using System;
using Microsoft.Extensions.Configuration;

namespace ServiceHost.Installer
{
    public sealed class InstallChecker
    {
        public bool IsInstallationStart { get; }

        public InstallChecker(IConfiguration configuration)
        {
            try
            {
                IsInstallationStart = configuration["Install"].ToLower() == "manual";
            }
            catch
            {
                IsInstallationStart = false;
            }
        }
    }
}