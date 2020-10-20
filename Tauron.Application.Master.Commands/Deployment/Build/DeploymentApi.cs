using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.AkkNode.Services.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Build
{
    [PublicAPI]
    public sealed class DeploymentApi : ISender
    {
        public const string DeploymentPath = @"DeploymentManager";

        private readonly IActorRef _repository;

        private DeploymentApi(IActorRef repository) => _repository = repository;

        public static DeploymentApi CreateProxy(ActorSystem system, string name = "DeploymentProxy")
        {
            var proxy = ClusterSingletonProxy.Props($"/user/{DeploymentPath}", ClusterSingletonProxySettings.Create(system).WithRole("UpdateSystem"));
            return new DeploymentApi(system.ActorOf(proxy, name));
        }

        void ISender.SendCommand(IReporterMessage command) => _repository.Tell(command);
    }
}