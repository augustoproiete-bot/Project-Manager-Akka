using Akka.Actor;
using Functional.Maybe;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public interface IDefaultActorRef<TActor> : IInitableActorRef
    {
    }

    [PublicAPI]
    public sealed class EmptyActor<TActor> : IDefaultActorRef<TActor>
    {
        public Maybe<IActorRef> Actor { get; } = Maybe<IActorRef>.Nothing;
        public void Init(string? name = null)
        {
            
        }

        public void Init(IActorRefFactory factory, string? name = null)
        {

        }
    }
}