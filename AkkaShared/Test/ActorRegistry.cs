using System.Collections.Generic;
using Akka.Actor;

namespace AkkaShared.Test
{
    public static class ActorRegistry
    {
        private static readonly Dictionary<string, IActorRef> _services = new Dictionary<string, IActorRef>();

        public static void Register(string name, IActorRef actor)
            => _services[name] = actor;

        public static bool TryGet(string name, out IActorRef service)
            => _services.TryGetValue(name, out service);
    }
}