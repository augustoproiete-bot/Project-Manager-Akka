using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Host
{
    [PublicAPI]
    public interface IHostLifetime
    {
        Task ShutdownTask { get; }
        Task WaitForStartAsync(ActorSystem actorSystem);
    }
}