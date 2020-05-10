using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public interface IInitableActorRef
    {
        IActorRef Actor { get; }

        void Init(string? name = null);

        void Init(IActorRefFactory factory, string? name = null);
    }
}