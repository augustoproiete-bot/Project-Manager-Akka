using System;
using JetBrains.Annotations;

namespace Tauron.Host
{
    [PublicAPI]
    public static class HostEnvironmentEnvExtensions
    {
        public static bool IsDevelopment(this IHostingEnvironment hostingEnvironment)
        {
            if (hostingEnvironment == null)
                throw new ArgumentNullException(nameof(hostingEnvironment));
            return hostingEnvironment.IsEnvironment(Environments.Development);
        }

        public static bool IsStaging(this IHostingEnvironment hostingEnvironment)
        {
            if (hostingEnvironment == null)
                throw new ArgumentNullException(nameof(hostingEnvironment));
            return hostingEnvironment.IsEnvironment(Environments.Staging);
        }

        public static bool IsProduction(this IHostingEnvironment hostingEnvironment)
        {
            if (hostingEnvironment == null)
                throw new ArgumentNullException(nameof(hostingEnvironment));
            return hostingEnvironment.IsEnvironment(Environments.Production);
        }

        public static bool IsEnvironment(this IHostingEnvironment hostingEnvironment, string environmentName)
        {
            if (hostingEnvironment == null)
                throw new ArgumentNullException(nameof(hostingEnvironment));
            return string.Equals(hostingEnvironment.EnvironmentName, environmentName, StringComparison.OrdinalIgnoreCase);
        }
    }
}