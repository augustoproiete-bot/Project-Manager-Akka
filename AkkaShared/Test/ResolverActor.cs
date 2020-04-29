using Akka.Actor;

namespace AkkaShared.Test
{
    public sealed class ResolverActor : ReceiveActor
    {
        public ResolverActor()
        {
            Receive<TryResolve>(TryResolve);
        }

        private void TryResolve(TryResolve obj)
        {
            var ok = ActorRegistry.TryGet(obj.Name, out var service);

            Context.Sender.Tell(new ResolveResult(service, obj.Name, ok));
        }
    }
}