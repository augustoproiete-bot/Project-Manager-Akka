using Akka.Actor;

namespace AkkaShared.Test
{
    public sealed class ResolveResult
    {
        public IActorRef Actor { get; }

        public string Name { get; }

        public bool Ok { get; }

        public ResolveResult(IActorRef actor, string name, bool ok)
        {
            Actor = actor;
            Name = name;
            Ok = ok;
        }
    }
}