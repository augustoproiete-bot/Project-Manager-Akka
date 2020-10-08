using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using JetBrains.Annotations;

namespace Tauron.Application.Master.Commands.Deployment.Build
{
    [PublicAPI]
    public sealed class DeploymentApi
    {
        public const string RepositoryPath = @"DeplaymentManager";

        private readonly IActorRef _repository;

        private DeploymentApi(IActorRef repository) => _repository = repository;

        public static DeploymentApi CreateProxy(ActorSystem system, string name = "DeploymentProxy")
        {
            var proxy = ClusterSingletonProxy.Props($"/user/{RepositoryPath}", ClusterSingletonProxySettings.Create(system).WithRole("UpdateSystem"));
            return new DeploymentApi(system.ActorOf(proxy, name));
        }


        internal void SendAction(DeplaymentAction action)
            => _repository.Tell(action);
    }
}