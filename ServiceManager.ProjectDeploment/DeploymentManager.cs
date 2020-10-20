using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using JetBrains.Annotations;
using MongoDB.Driver;
using ServiceManager.ProjectDeployment.Actors;
using Tauron.Application.AkkNode.Services.FileTransfer;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.ProjectDeployment
{
    [PublicAPI]
    public sealed class DeploymentManager
    {
        public static readonly DeploymentManager Empty = new DeploymentManager(ActorRefs.Nobody); 

        public static DeploymentManager CreateInstance(IActorRefFactory factory, string connectionString, DataTransferManager manager, RepositoryApi api)
            => new DeploymentManager(factory.ActorOf(Props.Create(() => new DeploymentServerImpl(new MongoClient(connectionString), manager, api)), DeploymentApi.DeploymentPath));

        public static DeploymentManager InitDeploymentManager(ActorSystem actorSystem, string connectionString, DataTransferManager manager, RepositoryApi api) 
            => InitDeploymentManager(actorSystem, new MongoClient(connectionString), manager, api);

        public static DeploymentManager InitDeploymentManager(ActorSystem actorSystem, IMongoClient client, DataTransferManager manager, RepositoryApi api)
        {
            var repo = ClusterSingletonManager.Props(Props.Create(() => new DeploymentServerImpl(client, manager, api)),
                ClusterSingletonManagerSettings.Create(actorSystem).WithRole("UpdateSystem"));
            return new DeploymentManager(actorSystem.ActorOf(repo, DeploymentApi.DeploymentPath));
        }

        private readonly IActorRef _manager;

        private DeploymentManager(IActorRef manager) => _manager = manager;

        public bool IsOk => !_manager.IsNobody();

        public void Stop()
            => _manager.Tell(PoisonPill.Instance);
    }
}