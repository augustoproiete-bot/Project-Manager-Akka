using System;
using System.Linq.Expressions;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public static class ActorRefFactoryExtensions
    {
        public static IActorRef GetOrAdd<TActor>(this IActorContext context, string name) where TActor : ActorBase, new()
        {
            var child = context.Child(name);
            return child.Equals(ActorRefs.Nobody) ? context.ActorOf<TActor>(name) : child;
        }

        public static IActorRef ActorOf<TActor>(this IActorRefFactory fac, Expression<Func<TActor>> creator, string name) where TActor : ActorBase 
            => fac.ActorOf(Props.Create(creator), name);
    }
}