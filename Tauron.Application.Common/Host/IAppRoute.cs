using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;

namespace Tauron.Host
{
    public interface IAppRoute
    {
        Task WaitForStartAsync(ActorSystem actorSystem);
    }
}