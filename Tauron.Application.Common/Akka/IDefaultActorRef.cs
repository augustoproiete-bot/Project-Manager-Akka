using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public interface IDefaultActorRef<TActor> : IInitableActorRef
    {
    }
}