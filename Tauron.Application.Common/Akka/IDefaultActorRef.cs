using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public interface IDefaultActorRef<TActor> : IActorRef
    {
        void Init();

        void Init(IActorRefFactory factory);
    }
}