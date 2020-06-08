using Akka.Actor;
using Akka.DI.Core;
using Tauron.Localization.Actor;

namespace Tauron.Localization.Extension
{
    public sealed class LocExtension : IExtension
    {
        public IActorRef LocCoordinator { get; private set; } = ActorRefs.Nobody;

        internal LocExtension Init(ActorSystem system)
        {
            LocCoordinator = system.ActorOf(system.DI().Props(typeof(LocCoordinator)));
            return this;
        }
    }
}