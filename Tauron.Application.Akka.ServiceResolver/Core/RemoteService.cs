using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;
using Tauron.Application.Akka.ServiceResolver.Messages.Global;

namespace Tauron.Application.Akka.ServiceResolver.Core
{
    [PublicAPI]
    public sealed class RemoteService
    {
        public sealed class ServiceTerminated
        {
            public string Name { get; }

            public ServiceTerminated(string name) => Name = name;
        }

        private readonly Task<IActorRef> _ask;

        public RemoteService(ActorSystem actorSystem, string name, ICanWatch self, ILoggingAdapter log)
        {
            log.Info("Begin Resolve Remote Service {Name}", name);
            _ask = TryGetService(actorSystem, name, self, log);
        }

        private async Task<IActorRef> TryGetService(ActorSystem actorSystem, string name, ICanWatch self, ILoggingAdapter log)
        {
            log.Info("Query Entry From Resolver {Name}", name);
            var result = await actorSystem.GetServiceEntry(name);
            log.Info("Query Entry From Service Host {Name}", name);
            result = await GetActor(result, name, log)
               .Ask<QueryServiceResponse>(new QueryServiceRequest(name), TimeSpan.FromMinutes(1));

            log.Info("Rsolve Compled {Name}");
            var actor = GetActor(result, name, log);
            self.WatchWith(actor, new ServiceTerminated(name));

            return actor;
        }

        private IActorRef GetActor(QueryServiceResponse response, string name, ILoggingAdapter log)
        {
            if (response.Ok)
                return response.Service!;
            
            log.Info("Resolve Failed {Name}", name);
            throw new ResolverException($"No Service for {name} found");
        }

        public IActorRef Service => _ask.Result;
    }
}