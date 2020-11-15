using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public interface IEventActor
    {
        IActorRef OriginalRef { get; }

        IEventActor Register(HookEvent hookEvent);

        IEventActor Send(IActorRef actor, object send);
    }
}