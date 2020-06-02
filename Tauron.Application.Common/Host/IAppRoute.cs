using System.Threading.Tasks;
using Akka.Actor;

namespace Tauron.Host
{
    public interface IAppRoute
    {
        Task ShutdownTask { get; }
        Task WaitForStartAsync(ActorSystem actorSystem);
    }
}