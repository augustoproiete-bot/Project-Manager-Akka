using Akka.Actor;

namespace Tauron.Akka
{
    public interface IInitableActorRef
    {
        void Init(string? name = null);

        void Init(IActorRefFactory factory, string? name = null);
    }
}