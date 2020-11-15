using Akka.Actor;
using Functional.Maybe;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public interface IInitableActorRef
    {
        Maybe<IActorRef> Actor { get; }

        void Init(string? name = null);

        void Init(IActorRefFactory factory, string? name = null);
    }
}