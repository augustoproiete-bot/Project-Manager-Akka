using Akka.Actor;
using Akka.DI.Core;
using Tauron.Localization.Actor;

namespace Tauron.Localization.Provider
{
    public sealed class LocJsonProvider : ILocStoreProducer
    {
        private readonly ActorSystem _actorSystem;

        public LocJsonProvider(ActorSystem actorSystem) 
            => _actorSystem = actorSystem;

        public string Name => "Json";

        public Props GetProps() => _actorSystem.DI().Props(typeof(JsonLocLocStoreActor));
    }
}