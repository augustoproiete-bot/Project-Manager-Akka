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
            => (ResolverExt) system.RegisterExtension(ResolverExtension.Id);

        public static Task<QueryServiceResponse> GetServiceEntry(this ActorSystem actorSystem, string name)
            => actorSystem.GetExtension<ResolverExt>().GetServiceEntry(name);

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
            var r = context.Child(name);
            if (r.Equals(ActorRefs.Nobody))
                return context.ActorOf(props, name);

            return r;
        }
    }
}