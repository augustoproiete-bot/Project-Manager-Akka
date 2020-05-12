using Akka.Actor;

namespace Tauron.Akka
{
    public static class ActorRefFactoryExtensions
    {
        public static IActorRef GetOrAdd<TActor>(this IActorContext context, string name) where TActor : ActorBase, new()
        {
            var child = context.Child(name);
            return child.Equals(ActorRefs.Nobody) ? context.ActorOf<TActor>(name) : child;
        }
    }
}