using JetBrains.Annotations;

namespace Tauron.Host
{
    [PublicAPI]
    public interface IHostEnvironment
    {
        string EnvironmentName { get; set; }

        string ApplicationName { get; set; }

        string ContentRootPath { get; set; }
    }
}