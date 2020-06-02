using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public interface ISyncActorRef<TActor> : IInitableActorRef
    {
    }
}