using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using JetBrains.Annotations;

namespace Tauron.Application.Master.Commands.Repository
{
    [PublicAPI]
    public sealed class RepositoryApi
    {
        public const string RepositoryPath = @"UpdateRepository";

        private readonly IActorRef _repository;

        private RepositoryApi(IActorRef repository) => _repository = repository;

        public static RepositoryApi CreateProxy(ActorSystem system, string name = "RepositoryProxy")
        {
            var proxy = ClusterSingletonProxy.Props($"/user/{RepositoryPath}", ClusterSingletonProxySettings.Create(system).WithRole("UpdateSystem"));
            return new RepositoryApi(system.ActorOf(proxy, name));
        }
    }
}