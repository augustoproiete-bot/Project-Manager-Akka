using Functional.Maybe;
using System;
using System.Linq.Expressions;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public static class ActorRefFactoryExtensions
    {
        public static Maybe<IActorRef> GetOrAdd<TActor>(this IActorContext context, Maybe<string> name) where TActor : ActorBase, new() 
            => GetOrAdd(context, name, Props.Create<TActor>());

        public static Maybe<IActorRef> GetOrAdd(this IActorContext context, Maybe<string> name, Props props)
        {
            return
                from realName in name
                from child in TryGetChild(context, name)

                select child.IsNobody() ?
                    context.ActorOf(props, realName) 
                    : child;
        }

        public static Maybe<IActorRef> ActorOf<TActor>(this IActorRefFactory fac, Expression<Func<TActor>> creator, string? name) where TActor : ActorBase
            => fac.ActorOf(Props.Create(creator), name).ToMaybe();

        public static Maybe<IActorRef> TryGetChild(this IActorContext context, Maybe<string> name)
        {
            return
            (
                from realName in name
                select string.IsNullOrWhiteSpace(realName) ? ActorRefs.Nobody : context.Child(realName)
            ).Or(ActorRefs.Nobody);
        }
    }
}