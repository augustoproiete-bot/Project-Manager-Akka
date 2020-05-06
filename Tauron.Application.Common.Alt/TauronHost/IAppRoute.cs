using System.Threading;
using System.Threading.Tasks;

namespace Tauron.TauronHost
{
    public interface IAppRoute
    {
        Task WaitForStartAsync(CancellationToken cancellationToken);

        Task StopAsync(CancellationToken cancellationToken);
    }
}