using System;
using System.Threading.Tasks;
using Akka.Actor;
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

        public RemoteService(ActorSystem actorSystem, string name, ICanWatch self)
        {
            _ask = TryGetService(actorSystem, name, self);
        }

        private async Task<IActorRef> TryGetService(ActorSystem actorSystem, string name, ICanWatch self)
        {
            var result = await actorSystem.GetServiceEntry(name);
            result = await GetActor(result, name)
               .Ask<QueryServiceResponse>(new QueryServiceRequest(name), TimeSpan.FromMinutes(1));

            var actor = GetActor(result, name);
            self.WatchWith(actor, new ServiceTerminated(name));

            return actor;
        }

        private IActorRef GetActor(QueryServiceResponse response, string name)
        {
            if (response.Ok)
                return response.Service!;
            
            throw new ResolverException($"No Service for {name} found");
        }

        public IActorRef Service => _ask.Result;
    }
}