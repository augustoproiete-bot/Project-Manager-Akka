using Akka.Actor;
using Akka.DI.Core;
using Functional.Maybe;
using JetBrains.Annotations;
using static Tauron.Prelude;

namespace Tauron.Akka
{
    [PublicAPI]
    public sealed class ActorRefFactory<TActor>
    {
        private readonly ActorSystem _system;

        public ActorRefFactory(ActorSystem system)
            => _system = system;

        public Maybe<IActorRef> Create(bool sync, string? name = null)
            => from prop in CreateProps(sync)
               select _system.ActorOf(prop, name);

        public Maybe<Props> CreateProps(bool sync)
            => from prop in May(_system.DI().Props(typeof(TActor)))
               select sync ? prop.WithDispatcher("synchronized-dispatcher") : prop;
    }
}