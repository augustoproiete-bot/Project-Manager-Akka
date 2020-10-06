using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.CleanUp;

namespace Tauron.Application.Master.Commands.Deployment.Repository
{
    [PublicAPI]
    public sealed class RepositoryApi
    {
        public const string RepositoryPath = @"RepositoryManager";

        private readonly IActorRef _repository;

        private RepositoryApi(IActorRef repository) => _repository = repository;

        public static RepositoryApi CreateProxy(ActorSystem system, string name = "RepositoryProxy")
        {
            var proxy = ClusterSingletonProxy.Props($"/user/{RepositoryPath}", ClusterSingletonProxySettings.Create(system).WithRole("UpdateSystem"));
            return new RepositoryApi(system.ActorOf(proxy, name));
        }

        public void CleanUp()
            => _repository.Tell(new StartCleanUp());

        public void SendAction(RepositoryAction action)
            => _repository.Tell(action);
    }
}