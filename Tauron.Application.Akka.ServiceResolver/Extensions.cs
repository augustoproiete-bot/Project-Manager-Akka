using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Akka.ServiceResolver.Core;
using Tauron.Application.Akka.ServiceResolver.Messages;
using Tauron.Application.Akka.ServiceResolver.Messages.Global;

namespace Tauron.Application.Akka.ServiceResolver
{
    [PublicAPI]
    public static class Extensions
    {
        public static ResolverExt AddServiceResolver(this ActorSystem system)
        {
            return (ResolverExt) system.RegisterExtension(ResolverExtension.Id);
        }

        public static Task<QueryServiceResponse> GetServiceEntry(this ActorSystem actorSystem, string name)
        {
            return actorSystem.GetExtension<ResolverExt>().GetServiceEntry(name);
        }

        public static RemoteService ResolveRemoteService(this IActorContext context, string name)
        {
            return context.System
                .GetExtension<ResolverExt>()
                .RemoteServiceActor
                .Ask<RemoteServiceResponse>(new RemoteServiceRequest(name))
                .Result.Service;
        }

        public static IActorRef GetOrCreate(this IActorContext context, string name, Props props)
        {
            var actor = context.Child(name);
            return actor.Equals(ActorRefs.Nobody) ? context.ActorOf(props, name) : actor;
        }

        public static Task<IActorRef> HostLocalService(this ActorSystem system, string name, Props props)
        {
            return system
                .GetExtension<ResolverExt>()
                .HostActor.Ask<HostLocalServiceResponse>(new HostLocalServiceMessage(props, name))
                .ContinueWith(t =>
                {
                    if (t.Result.Service == null)
                        throw new ResolverException("Local Host is Aready Regitrated");
                    return t.Result.Service;
                }, TaskContinuationOptions.RunContinuationsAsynchronously);
        }

        public static ICanTell Combine(this ICanTell? teller, params ICanTell?[] toCombine)
        {
            ICanTell?[] temp = new ICanTell[toCombine.Length + 1];

            toCombine.CopyTo(temp, 1);
            temp[0] = teller;

            return new TellCombiner(temp);
        }

        private class TellCombiner : ICanTell
        {
            private readonly ICanTell?[] _teller;

            public TellCombiner(ICanTell?[] teller)
            {
                _teller = teller;
            }

            public void Tell(object message, IActorRef sender)
            {
                foreach (var tell in _teller)
                    tell?.Tell(message, sender);
            }
        }
    }
}