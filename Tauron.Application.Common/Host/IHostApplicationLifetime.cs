using System.Threading;
using JetBrains.Annotations;

namespace Tauron.Host
{
    [PublicAPI]
    public interface IHostApplicationLifetime
    {
        CancellationToken ApplicationStarted { get; }

        CancellationToken ApplicationStopping { get; }

        CancellationToken ApplicationStopped { get; }

        void StopApplication();
    }
}